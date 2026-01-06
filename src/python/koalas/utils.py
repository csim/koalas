import json
import re
import shutil
import xml.etree.ElementTree as ET
from pathlib import Path
from typing import Any

import rich


def find_project_paths(base_dir: Path | str, project_filter: str | None = None) -> list[Path]:
    """Find all .csproj files recursively from a base directory.

    Args:
        base_dir: Base directory to search for projects
        project_filter: Optional glob pattern(s) to filter project paths.
                       Multiple patterns can be separated by semicolons.
                       Patterns starting with ! are exclusions.
                       (e.g., "**/Transformation.Formula;!**/Semantics/**")
    """
    base_path = Path(base_dir)
    project_dirs = [csproj.parent for csproj in base_path.rglob("*.csproj")]
    # Filter to only include directories with packages.lock.json
    packages_paths = [p for p in project_dirs if (p / "packages.lock.json").exists()]

    # Apply glob filter if provided
    if project_filter:
        patterns = [p.strip() for p in project_filter.split(";")]
        include_patterns = [p for p in patterns if not p.startswith("!")]
        exclude_patterns = [p[1:] for p in patterns if p.startswith("!")]

        # Apply include patterns (if any)
        if include_patterns:
            packages_paths = [p for p in packages_paths if any(p.match(pattern) for pattern in include_patterns)]

        # Apply exclude patterns
        if exclude_patterns:
            packages_paths = [p for p in packages_paths if not any(p.match(pattern) for pattern in exclude_patterns)]

    return sorted(packages_paths)


def capture_before_deps(base_dir: Path):
    """Copy packages.lock.json to packages.before.lock.json"""
    project_paths = find_project_paths(base_dir)
    # Convert single path to list for uniform processing
    if isinstance(project_paths, Path):
        project_paths = [project_paths]

    for project_path in project_paths:
        source = project_path / "packages.lock.json"
        destination = project_path / "packages.before.lock.json"

        if source.exists():
            shutil.copy2(source, destination)
            print(f"Copied {source} to {destination}")
        else:
            print(f"Warning: {source} does not exist")


def remove_before_deps(base_dir: Path | str):
    """Remove all packages.before.lock.json files from base directory"""
    base_path = Path(base_dir)
    before_files = list(base_path.rglob("packages.before.lock.json"))

    for before_file in before_files:
        before_file.unlink()
        print(f"Removed {before_file}")

    print(f"Total files removed: {len(before_files)}")


def load_deps(file_path: Path, framework: str, project_path: Path) -> dict[str, Any]:
    """Load and filter dependencies from a packages.lock.json file."""
    with open(file_path) as f:
        data = json.load(f)
    if framework not in data["dependencies"]:
        rich.print(f"{project_path}: [yellow]skipping, {framework} not available[/yellow]")
        return {}
    deps = data["dependencies"][framework]
    return {k.lower(): v for k, v in deps.items() if not k.startswith("runtime") and v["type"] != "Project"}


def load_global_deps(global_package_path: Path | None) -> dict[str, str]:
    """Parse global package versions from .props or .packageset file."""
    if not global_package_path:
        return {}

    if global_package_path.suffix == ".props":
        global_xml = ET.parse(global_package_path).getroot()
        ret = {}
        for pkg in global_xml.findall(".//PackageVersion"):
            key = pkg.get("Include")
            value = pkg.get("Version") or ""
            if key:
                ret[key.lower()] = value
        return ret
    elif global_package_path.suffix == ".packageset":
        with open(global_package_path) as f:
            global_json = json.load(f)
        return {pkg["id"].lower(): pkg["version"] for pkg in global_json["packages"]["nuget"]}

    return {}


def parse_framework_version(framework: str) -> str:
    """Extract version number from framework string."""
    # Match patterns like "net8.0" -> "8.0" or ".NETFramework,Version=v4.7.2" -> "4.7.2"
    match = re.search(r"(\d+\.\d+(?:\.\d+)?)", framework)
    return match.group(1) if match else framework
