from collections import defaultdict
from dataclasses import dataclass
from enum import Enum
from pathlib import Path


class DependencyType(Enum):
    DIRECT = "Direct"
    TRANSITIVE = "Transitive"
    CENTRAL_TRANSITIVE = "CentralTransitive"
    PROJECT = "Project"
    UNKNOWN = "Unknown"


@dataclass
class PackageUsage:
    name: str
    name_lower: str
    type: DependencyType
    framework_version: str
    requested_version: str
    resolved_version: str
    dependencies: dict[str, str] | None = None  # package_name -> version


@dataclass
class PackageSummary:
    project_path: Path
    packages_file_path: Path
    usages: list[PackageUsage]

    @property
    def lookup(self) -> dict[str, list[PackageUsage]]:
        """Returns a dictionary mapping package names (lowercase) to their usages."""
        if not hasattr(self, "_lookup_cache"):
            result = defaultdict(list)
            for usage in self.usages:
                result[usage.name_lower].append(usage)
            self._lookup_cache = dict(result)
        return self._lookup_cache
