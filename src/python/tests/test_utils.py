"""Tests for koalas.utils module."""

import json
from pathlib import Path

from koalas.utils import (
    _find_project_paths,
    _load_global_deps,
    _parse_framework_version,
    capture_before_deps,
    remove_before_deps,
)


class TestFindProjectPaths:
    """Tests for _find_project_paths function."""

    def test_find_single_project(self, temp_project_dir: Path, sample_packages_lock_json: Path):
        """Test finding a single project with packages.lock.json."""
        base_dir = temp_project_dir.parent
        projects = _find_project_paths(base_dir)

        assert len(projects) == 1
        assert projects[0] == temp_project_dir

    def test_find_no_projects_without_lock_file(self, tmp_path: Path):
        """Test that projects without packages.lock.json are not found."""
        project_dir = tmp_path / "NoLockFile"
        project_dir.mkdir()
        (project_dir / "Project.csproj").write_text("<Project />")

        projects = _find_project_paths(tmp_path)

        assert len(projects) == 0

    def test_filter_with_include_pattern(self, tmp_path: Path):
        """Test filtering projects with include pattern."""
        # Create two projects
        proj1 = tmp_path / "Transformation.Formula" / "Formula"
        proj1.mkdir(parents=True)
        (proj1 / "Formula.csproj").write_text("<Project />")
        (proj1 / "packages.lock.json").write_text("{}")

        proj2 = tmp_path / "Other" / "Project"
        proj2.mkdir(parents=True)
        (proj2 / "Project.csproj").write_text("<Project />")
        (proj2 / "packages.lock.json").write_text("{}")

        projects = _find_project_paths(tmp_path, project_filter="**/Transformation.Formula/**")

        assert len(projects) == 1
        assert "Transformation.Formula" in str(projects[0])

    def test_filter_with_exclude_pattern(self, tmp_path: Path):
        """Test filtering projects with exclude pattern."""
        # Create two projects
        proj1 = tmp_path / "Include" / "Project1"
        proj1.mkdir(parents=True)
        (proj1 / "Project1.csproj").write_text("<Project />")
        (proj1 / "packages.lock.json").write_text("{}")

        proj2 = tmp_path / "Exclude" / "Project2"
        proj2.mkdir(parents=True)
        (proj2 / "Project2.csproj").write_text("<Project />")
        (proj2 / "packages.lock.json").write_text("{}")

        projects = _find_project_paths(tmp_path, project_filter="!**/Exclude/**")

        assert len(projects) == 1
        assert "Include" in str(projects[0])


class TestParseFrameworkVersion:
    """Tests for _parse_framework_version function."""

    def test_parse_net_version(self):
        """Test parsing modern .NET version."""
        assert _parse_framework_version("net8.0") == "8.0"
        assert _parse_framework_version("net6.0") == "6.0"

    def test_parse_framework_version_string(self):
        """Test parsing .NET Framework version string."""
        assert _parse_framework_version(".NETFramework,Version=v4.7.2") == "4.7.2"
        assert _parse_framework_version(".NETFramework,Version=v4.8") == "4.8"

    def test_parse_netstandard_version(self):
        """Test parsing .NET Standard version."""
        assert _parse_framework_version("netstandard2.0") == "2.0"
        assert _parse_framework_version("netstandard2.1") == "2.1"

    def test_parse_unknown_format(self):
        """Test parsing unknown format returns original string."""
        unknown = "unknown-framework"
        assert _parse_framework_version(unknown) == unknown


class TestLoadGlobalDeps:
    """Tests for _load_global_deps function."""

    def test_load_from_props_file(self, sample_directory_packages_props: Path):
        """Test loading global dependencies from .props file."""
        deps = _load_global_deps(sample_directory_packages_props)

        assert "newtonsoft.json" in deps
        assert deps["newtonsoft.json"] == "13.0.3"
        assert "system.text.json" in deps
        assert deps["system.text.json"] == "8.0.1"

    def test_load_from_packageset_file(self, tmp_path: Path):
        """Test loading global dependencies from .packageset file."""
        packageset_data = {
            "packages": {
                "nuget": [
                    {"id": "Newtonsoft.Json", "version": "13.0.3"},
                    {"id": "System.Text.Json", "version": "8.0.1"},
                ]
            }
        }
        packageset_file = tmp_path / "test.packageset"
        packageset_file.write_text(json.dumps(packageset_data))

        deps = _load_global_deps(packageset_file)

        assert "newtonsoft.json" in deps
        assert deps["newtonsoft.json"] == "13.0.3"

    def test_load_with_none_path(self):
        """Test loading with None path returns empty dict."""
        deps = _load_global_deps(None)
        assert deps == {}

    def test_load_with_nonexistent_file(self, tmp_path: Path):
        """Test loading nonexistent file returns empty dict."""
        deps = _load_global_deps(tmp_path / "nonexistent.props")
        assert deps == {}


class TestCaptureBeforeDeps:
    """Tests for capture_before_deps function."""

    def test_capture_creates_before_file(self, temp_project_dir: Path, sample_packages_lock_json: Path):
        """Test that capture creates a before file."""
        capture_before_deps(temp_project_dir.parent)

        before_file = temp_project_dir / "packages.before.lock.json"
        assert before_file.exists()

        # Verify content matches
        original = json.loads(sample_packages_lock_json.read_text())
        captured = json.loads(before_file.read_text())
        assert original == captured


class TestRemoveBeforeDeps:
    """Tests for remove_before_deps function."""

    def test_remove_before_files(self, temp_project_dir: Path):
        """Test removing before files."""
        # Create a before file
        before_file = temp_project_dir / "packages.before.lock.json"
        before_file.write_text("{}")

        assert before_file.exists()

        remove_before_deps(temp_project_dir.parent)

        assert not before_file.exists()
