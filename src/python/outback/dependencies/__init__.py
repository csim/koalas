"""Outback - Python utilities for .NET package dependency analysis and management."""

from .print import (
    print_package_diffs,
    print_project_summary,
    print_projects,
)

__version__ = "0.1.0"

__all__ = [
    "print_package_diffs",
    "print_project_summary",
    "print_projects",
]
