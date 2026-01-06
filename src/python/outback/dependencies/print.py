import json
import re
from pathlib import Path

import rich
from rich.table import Table
from rich.tree import Tree

from outback.dependencies.types import DependencyType, PackageSummary, PackageUsage

from .utils import (
    find_project_paths,
    load_global_deps,
    parse_framework_version,
)

type_map = {
    "Direct": DependencyType.DIRECT,
    "Transitive": DependencyType.TRANSITIVE,
    "CentralTransitive": DependencyType.CENTRAL_TRANSITIVE,
    "Project": DependencyType.PROJECT,
}


def _capitalize_name(name: str) -> str:
    return re.sub(r"(?:^|(?<=\.))(.)(?=\w)", lambda m: m.group(1).upper(), name)


def _package_summary(packages_file_path: Path, include_transitive: bool = False) -> PackageSummary:
    """
    Analyze package usage across multiple .NET projects.

    Args:
        base_dir: Base directory to search for projects
        package_filename: Name of the lock file to parse (default: packages.lock.json)
        include_transitive: Whether to include transitive dependencies (default: True)
        project_filter: Optional regex pattern to filter project paths

    Returns:
        PackageSummary: Strongly-typed model containing:
            - usages: List of all PackageUsage instances
            - project_lookup: Dict mapping project paths to their package usages
            - package_lookup: Dict mapping package names to their usages across projects
    """
    usages: list[PackageUsage] = []

    if not packages_file_path.exists():
        raise Exception(f"Packages file not found: {packages_file_path}")

    with open(packages_file_path) as f:
        package_json = json.load(f)

    # Iterate through all frameworks
    for framework, deps in package_json.get("dependencies", {}).items():
        # Parse framework version once here
        parsed_framework = parse_framework_version(framework)
        # Check each dependency
        for dep_name, dep_info in deps.items():
            dep_type_str = dep_info.get("type", "Unknown")
            dep_type = type_map.get(dep_type_str, DependencyType.UNKNOWN)

            if not include_transitive and dep_type == DependencyType.TRANSITIVE:
                continue

            dep_name_lower = dep_name.lower()
            requested_version = dep_info.get("requested", "")
            resolved_version = dep_info.get("resolved", "")

            # Extract nested dependencies
            nested_deps = dep_info.get("dependencies", {})

            usage = PackageUsage(
                name_lower=dep_name_lower,
                name=dep_name,
                type=dep_type,
                framework_version=parsed_framework,
                requested_version=requested_version,
                resolved_version=resolved_version,
                dependencies=nested_deps if nested_deps else None,
            )

            usages.append(usage)

    return PackageSummary(project_path=packages_file_path, packages_file_path=packages_file_path, usages=usages)


def _print_projects_add_package(
    parent_node,
    summary: PackageSummary,
    usage: PackageUsage,
    badge_map: dict,
    framework: str,
    max_name_length: int,
    visited: set[str] | None = None,
    indent_level: int = 0,
    include_transitive: bool = False,
):
    """
    Recursively add package nodes with their dependencies.

    Args:
        parent_node: The parent tree node to add to
        usage: The PackageUsage to display
        max_name_length: Maximum package name length for formatting
        badge_map: Mapping of DependencyType to badge strings
        package_lookup: Lookup dict to find dependency details
        framework: Current framework version
        visited: Set of visited package names to prevent cycles
        indent_level: Current indentation level for formatting
    """
    if visited is None:
        visited = set()

    # Prevent infinite loops
    if usage.name_lower in visited:
        return
    visited.add(usage.name_lower)

    color = (
        "green"
        if usage.type == DependencyType.DIRECT
        else "yellow"
        if usage.type == DependencyType.CENTRAL_TRANSITIVE
        else "white"
        if usage.type == DependencyType.PROJECT
        else "dim"
        if usage.type == DependencyType.TRANSITIVE
        else "red"
    )
    badge = badge_map[usage.type]
    name_display = _capitalize_name(usage.name)
    padding = (max_name_length + 1) - (indent_level * 4)
    pkg_label = f"[{color}][{badge}][/{color}] {name_display:<{padding}} {usage.resolved_version}"
    pkg_node = parent_node.add(pkg_label)

    # Add nested dependencies if they exist
    if usage.dependencies:
        for dep_name, dep_version in sorted(usage.dependencies.items()):
            dep_name_lower = dep_name.lower()
            dep_usages = summary.lookup.get(dep_name_lower, [])
            matching_usage = next((u for u in dep_usages if u.framework_version == framework), None)

            # If no exact framework match, try any usage from this package
            if not matching_usage and dep_usages:
                matching_usage = dep_usages[0]

            if matching_usage:
                # Recursively add the dependency and its dependencies
                _print_projects_add_package(
                    pkg_node,
                    summary,
                    matching_usage,
                    badge_map,
                    framework,
                    max_name_length,
                    visited.copy(),  # Copy to allow different branches to revisit packages
                    indent_level + 1,
                )
            elif include_transitive:
                # Dependency not found in package lookup - show as transitive
                pkg_node.add(f"[cyan][T ][/cyan] [dim]{_capitalize_name(dep_name)} {dep_version}[/dim]")


def print_projects(
    base_dir: Path,
    packages_file_name: str = "packages.lock.json",
    include_transitive: bool = False,
    project_filter: str | None = None,
    nested: bool = True,
):
    """
    Display dependency graph showing project -> package relationships with nested dependencies.

    Args:
        base_directory: Base directory to search for projects
        project_filter: Optional glob pattern to filter project paths (e.g., "*/Transformation.*/*")
        nested: Show nested dependencies (True) or flat list (False)
    """
    project_paths = find_project_paths(base_dir, project_filter=project_filter)

    badge_map = {
        DependencyType.DIRECT: "D ",
        DependencyType.CENTRAL_TRANSITIVE: "CT",
        DependencyType.TRANSITIVE: "T ",
        DependencyType.PROJECT: "P ",
        DependencyType.UNKNOWN: "? ",
    }

    # Use project_lookup directly - already grouped by project
    for project_path in sorted(project_paths):
        summary = _package_summary(project_path / packages_file_name, include_transitive=include_transitive)
        usages = summary.usages

        # Create a new tree for each project
        tree = Tree(f"[bold cyan]Project Dependencies[/bold cyan] - {project_path}")
        project_node = tree

        # Group by framework
        framework_groups: dict[str, list[PackageUsage]] = {}
        for usage in usages:
            if usage.framework_version not in framework_groups:
                framework_groups[usage.framework_version] = []
            framework_groups[usage.framework_version].append(usage)

        for framework in sorted(framework_groups.keys()):
            framework_node = project_node.add(f"{framework}")
            packages = framework_groups[framework]

            # Calculate max package name length for this framework
            max_name_length = max((len(_capitalize_name(p.name)) for p in packages), default=0)

            if nested:
                # Show nested dependencies
                top_level_packages = [p for p in packages if p.type in (DependencyType.DIRECT, DependencyType.PROJECT)]
                for usage in sorted(top_level_packages, key=lambda x: x.name_lower):
                    _print_projects_add_package(
                        framework_node,
                        summary,
                        usage,
                        badge_map,
                        framework,
                        max_name_length,
                        include_transitive=include_transitive,
                    )
            else:
                # Show flat list
                for usage in sorted(packages, key=lambda x: x.name_lower):
                    color = (
                        "green"
                        if usage.type == DependencyType.DIRECT
                        else "yellow"
                        if usage.type == DependencyType.CENTRAL_TRANSITIVE
                        else "white"
                        if usage.type == DependencyType.PROJECT
                        else "dim"
                    )
                    badge = badge_map[usage.type]
                    name_display = _capitalize_name(usage.name)
                    pkg_label = (
                        f"[{color}][{badge}][/{color}] {name_display:<{max_name_length}} {usage.resolved_version}"
                    )
                    framework_node.add(pkg_label)

        rich.print(tree)


def _format_version_display(usage: PackageUsage | None, highlight: bool = False, absent_value: str = "") -> str:
    """Helper to format version display for diff tables."""
    if not usage:
        return absent_value
    if usage.type == DependencyType.PROJECT:
        return "[dim]Project[/dim]\n"
    color = "yellow" if highlight else "white"
    return f"[dim]{usage.type.value}[/dim]\n[{color}]{usage.requested_version}\n  → {usage.resolved_version}[/{color}]"


def print_package_diffs(
    base_dir: Path,
    before_file: str = "packages.before.lock.json",
    after_file: str = "packages.lock.json",
    global_version_path: Path | None = None,
    only_changes: bool = False,
    project_filter: str | None = None,
    include_transitive: bool = True,
):
    """Print package differences between before and after states.

    Args:
        base_dir: Base directory to search for projects
        before_file: Name of the before lock file
        after_file: Name of the after lock file
        global_version_path: Path to global package versions file (.props or .packageset)
        only_changes: Only show packages that changed
        project_filter: Optional glob pattern to filter project paths (e.g., "*/Transformation.*/*")
        include_transitive: Include transitive dependencies
    """
    # Load global package versions
    global_versions = load_global_deps(global_version_path) if global_version_path else {}

    project_paths = find_project_paths(base_dir, project_filter=project_filter)

    # Process each project
    for project_path in sorted(project_paths):
        before_path = project_path / before_file
        after_path = project_path / after_file

        # Skip if files don't exist
        if not before_path.exists() or not after_path.exists():
            continue

        before_summary = _package_summary(before_path, include_transitive=include_transitive)
        after_summary = _package_summary(after_path, include_transitive=include_transitive)

        # Collect all frameworks from both summaries
        frameworks = set()
        for usage in before_summary.usages + after_summary.usages:
            frameworks.add(usage.framework_version)

        # Process each framework
        for framework in sorted(frameworks):
            # Build lookup dicts for this project/framework combination
            before_by_name = {u.name_lower: u for u in before_summary.usages if u.framework_version == framework}
            after_by_name = {u.name_lower: u for u in after_summary.usages if u.framework_version == framework}

            # Build dependency rows
            rows = []
            all_pkg_names = set(before_by_name.keys()) | set(after_by_name.keys())

            for pkg_name in sorted(all_pkg_names):
                before_pkg = before_by_name.get(pkg_name)
                after_pkg = after_by_name.get(pkg_name)

                has_change = before_pkg != after_pkg

                global_diff = (
                    after_pkg
                    and global_versions.get(pkg_name)
                    and after_pkg.resolved_version != global_versions.get(pkg_name)
                ) or False

                if only_changes and not has_change:
                    continue
                if not (pkg := (after_pkg or before_pkg)):
                    continue
                pkg_name_display = _capitalize_name(pkg.name)

                before_display = _format_version_display(before_pkg)
                after_display = _format_version_display(
                    after_pkg, highlight=has_change, absent_value="\n\n[yellow]removed[/yellow]"
                )

                global_display = ""
                if global_ver := global_versions.get(pkg_name):
                    color = "yellow" if global_diff else "green"
                    global_display = f"\n\n[{color}]{global_ver}[/{color}]"

                rows.append((pkg_name_display, before_display, after_display, global_display))

            if not rows:
                continue

            title = [
                f"[cyan]Dependency diff ({framework})[/cyan]",
                f"project: {project_path}",
                f"         {before_file} -> {after_file}",
                f"global: {global_version_path.name if global_version_path else 'N/A'}",
            ]

            table = Table(
                title="\n".join(title),
                title_justify="left",
            )
            table.add_column(f"Package   ({len(rows)})", style="white")
            table.add_column("Before", no_wrap=True)
            table.add_column("After", no_wrap=True)
            table.add_column("Global", no_wrap=True)
            rows.sort(key=lambda x: x[0])
            for row in rows:
                table.add_row(*row)
                table.add_row()
            rich.print(table)


def print_project_summary(
    base_dir: Path,
    global_version_path: Path,
    flat: bool = False,
    project_filter: str | None = None,
    only_changes: bool = False,
):
    nested = not flat
    include_transitive = flat

    print_projects(base_dir, nested=nested, include_transitive=include_transitive, project_filter=project_filter)

    print_package_diffs(
        base_dir,
        global_version_path=global_version_path,
        only_changes=only_changes,
        include_transitive=include_transitive,
        project_filter=project_filter,
    )
