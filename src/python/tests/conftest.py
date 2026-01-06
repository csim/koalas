"""Test configuration and shared fixtures for koalas tests."""

import json
from pathlib import Path

import pytest


@pytest.fixture
def temp_project_dir(tmp_path: Path) -> Path:
    """Create a temporary project directory structure."""
    project_dir = tmp_path / "TestProject"
    project_dir.mkdir()

    # Create a .csproj file
    csproj_content = """<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
</Project>"""
    (project_dir / "TestProject.csproj").write_text(csproj_content)

    return project_dir


@pytest.fixture
def sample_packages_lock_json(temp_project_dir: Path) -> Path:
    """Create a sample packages.lock.json file."""
    packages_data = {
        "version": 1,
        "dependencies": {
            "net8.0": {
                "Newtonsoft.Json": {
                    "type": "Direct",
                    "requested": "[13.0.1, )",
                    "resolved": "13.0.3",
                    "contentHash": "abc123",
                },
                "System.Text.Json": {
                    "type": "Transitive",
                    "requested": "[8.0.0, )",
                    "resolved": "8.0.1",
                    "contentHash": "def456",
                    "dependencies": {"System.Runtime": "8.0.0"},
                },
                "MyProject.Core": {"type": "Project"},
            }
        },
    }

    lock_file = temp_project_dir / "packages.lock.json"
    lock_file.write_text(json.dumps(packages_data, indent=2))
    return lock_file


@pytest.fixture
def sample_directory_packages_props(tmp_path: Path) -> Path:
    """Create a sample Directory.Packages.props file."""
    props_content = """<?xml version="1.0" encoding="utf-8"?>
<Project>
  <ItemGroup>
    <PackageVersion Include="Newtonsoft.Json" Version="13.0.3" />
    <PackageVersion Include="System.Text.Json" Version="8.0.1" />
  </ItemGroup>
</Project>"""

    props_file = tmp_path / "Directory.Packages.props"
    props_file.write_text(props_content)
    return props_file
