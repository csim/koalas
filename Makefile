# Makefile for Koalas .NET Project
# =================================

# Variables
SOLUTION = src/Koalas.sln
SOLUTION_DIR = src
MAIN_PROJECT = src/Koalas/Koalas.csproj
CLI_PROJECT = src/Koalas.CommandLine/Koalas.CommandLine.csproj
TEST_PROJECT = src/Koalas.Tests/Koalas.Tests.csproj
CONFIGURATION = Release
OUTPUT_DIR = artifacts
PUBLISH_DIR = $(OUTPUT_DIR)/publish
COVERAGE_DIR = $(OUTPUT_DIR)/coverage

# Default target
.PHONY: all
all: clean restore build test

# Help target
.PHONY: help
help:
	@echo "Available targets:"
	@echo "  all         - Clean, restore, build, and test"
	@echo "  clean       - Clean build artifacts"
	@echo "  restore     - Restore NuGet packages"
	@echo "  build       - Build the solution"
	@echo "  rebuild     - Clean and build"
	@echo "  test        - Run unit tests"
	@echo "  test-watch  - Run tests in watch mode"
	@echo "  coverage    - Run tests with coverage"
	@echo "  publish     - Publish the CLI application"
	@echo "  package     - Create NuGet packages"
	@echo "  run         - Run the CLI application"
	@echo "  format      - Format code using dotnet format"
	@echo "  lint        - Run code analysis"
	@echo "  install     - Install the CLI tool globally"
	@echo "  uninstall   - Uninstall the CLI tool globally"

# Clean targets
.PHONY: clean
clean:
	@echo "Cleaning build artifacts..."
	dotnet clean $(SOLUTION) --configuration $(CONFIGURATION)
	rm -rf $(OUTPUT_DIR)
	rm -rf src/*/bin src/*/obj

.PHONY: clean-all
clean-all: clean
	@echo "Cleaning all artifacts including packages..."
	dotnet nuget locals all --clear

# Restore dependencies
.PHONY: restore
restore:
	@echo "Restoring NuGet packages..."
	dotnet restore $(SOLUTION)

# Build targets
.PHONY: build
build:
	@echo "Building solution..."
	dotnet build $(SOLUTION) --configuration $(CONFIGURATION) --no-restore

.PHONY: build-debug
build-debug:
	@echo "Building solution (Debug)..."
	dotnet build $(SOLUTION) --configuration Debug --no-restore

.PHONY: rebuild
rebuild: clean restore build

# Test targets
.PHONY: test
test:
	@echo "Running unit tests..."
	dotnet test $(SOLUTION) --configuration $(CONFIGURATION) --no-build --verbosity normal

.PHONY: test-debug
test-debug:
	@echo "Running unit tests (Debug)..."
	dotnet test $(SOLUTION) --configuration Debug --no-build --verbosity normal

.PHONY: test-watch
test-watch:
	@echo "Running tests in watch mode..."
	dotnet watch test $(TEST_PROJECT)

.PHONY: coverage
coverage:
	@echo "Running tests with coverage..."
	mkdir -p $(COVERAGE_DIR)
	dotnet test $(SOLUTION) \
		--configuration $(CONFIGURATION) \
		--no-build \
		--collect:"XPlat Code Coverage" \
		--results-directory $(COVERAGE_DIR) \
		--verbosity normal

# Publish targets
.PHONY: publish
publish:
	@echo "Publishing CLI application..."
	dotnet publish $(CLI_PROJECT) \
		--configuration $(CONFIGURATION) \
		--output $(PUBLISH_DIR) \
		--self-contained false

.PHONY: publish-self-contained
publish-self-contained:
	@echo "Publishing self-contained CLI application..."
	dotnet publish $(CLI_PROJECT) \
		--configuration $(CONFIGURATION) \
		--output $(PUBLISH_DIR)/self-contained \
		--self-contained true \
		--runtime win-x64

# Package targets
.PHONY: package
package:
	@echo "Creating NuGet packages..."
	dotnet pack $(SOLUTION) \
		--configuration $(CONFIGURATION) \
		--no-build \
		--output $(OUTPUT_DIR)/packages

# Run targets
.PHONY: run
run:
	@echo "Running CLI application..."
	dotnet run --project $(CLI_PROJECT)

.PHONY: run-debug
run-debug:
	@echo "Running CLI application (Debug)..."
	dotnet run --project $(CLI_PROJECT) --configuration Debug

# Code quality targets
.PHONY: format
format:
	@echo "Formatting code..."
	dotnet csharpier format $(SOLUTION_DIRs) --log-format Console


.PHONY: format-check
format-check:
	@echo "Checking code formatting..."
	dotnet format $(SOLUTION) --verify-no-changes --verbosity normal

.PHONY: lint
lint:
	@echo "Running code analysis..."
	dotnet build $(SOLUTION) --configuration $(CONFIGURATION) --verbosity normal

# Install/Uninstall CLI tool
.PHONY: install
install: publish
	@echo "Installing CLI tool globally..."
	dotnet tool uninstall --global koalas || true
	dotnet tool install --global --add-source $(PUBLISH_DIR) koalas

.PHONY: uninstall
uninstall:
	@echo "Uninstalling CLI tool globally..."
	dotnet tool uninstall --global koalas

# Development targets
.PHONY: dev
dev: restore build-debug test-debug
	@echo "Development build complete!"

.PHONY: ci
ci: clean restore build test package
	@echo "CI build complete!"

# Watch targets
.PHONY: watch
watch:
	@echo "Starting file watcher for build..."
	dotnet watch build $(MAIN_PROJECT)

.PHONY: watch-run
watch-run:
	@echo "Starting file watcher for run..."
	dotnet watch run --project $(CLI_PROJECT)

# Info targets
.PHONY: info
info:
	@echo "Project Information:"
	@echo "  Solution: $(SOLUTION)"
	@echo "  Main Project: $(MAIN_PROJECT)"
	@echo "  CLI Project: $(CLI_PROJECT)"
	@echo "  Test Project: $(TEST_PROJECT)"
	@echo "  Configuration: $(CONFIGURATION)"
	@echo "  Output Directory: $(OUTPUT_DIR)"
	@echo ""
	@echo "Environment:"
	@dotnet --version
	@echo ""

.PHONY: version
version:
	@echo "Checking .NET version..."
	@dotnet --version
	@dotnet --list-sdks

# Utility targets
.PHONY: tree
tree:
	@echo "Project structure:"
	@find . -type f -name "*.csproj" -o -name "*.sln" | head -20

# Dependencies
package: build
publish: build
coverage: build
