"""Outback - Python utilities for .NET package dependency analysis and management."""

from .print import (
    print_package_diffs,
    print_project_summary,
    print_projects,
)

from .utils import capture_before_deps, remove_before_deps


__version__ = "0.1.0"

__all__ = [
    "print_package_diffs",
    "print_project_summary",
    "print_projects",
    "capture_before_deps",
    "remove_before_deps",
]
