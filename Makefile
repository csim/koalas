# Koalas .NET Project Makefile

# Variables
SOLUTION = src/Koalas.sln
CLI_PROJECT = src/Koalas.CommandLine/Koalas.CommandLine.csproj
TEST_PROJECT = src/Koalas.Tests/Koalas.Tests.csproj
CONFIGURATION = Release

# Default target
.PHONY: all
all: clean restore build test

# Basic build targets
.PHONY: clean
clean:
	@echo "Cleaning..."
	dotnet clean $(SOLUTION)

.PHONY: restore
restore:
	@echo "Restoring packages..."
	dotnet restore $(SOLUTION)

.PHONY: build
build:
	@echo "Building..."
	dotnet build $(SOLUTION) --configuration $(CONFIGURATION) --no-restore

.PHONY: rebuild
rebuild: clean restore build

# Test targets
.PHONY: test
test:
	@echo "Running tests..."
	dotnet test $(SOLUTION) --configuration $(CONFIGURATION) --no-build

# Run target
.PHONY: run
run:
	@echo "Running CLI..."
	dotnet run --project $(CLI_PROJECT)

# Publish target
.PHONY: publish
publish:
	@echo "Publishing..."
	dotnet publish $(CLI_PROJECT) --configuration $(CONFIGURATION) --output artifacts/publish

# Code quality
.PHONY: format
format:
	@echo "Formatting code..."
	dotnet format $(SOLUTION)

# Help
.PHONY: help
help:
	@echo "Available targets:"
	@echo "  all      - Clean, restore, build, and test"
	@echo "  clean    - Clean build artifacts"
	@echo "  restore  - Restore NuGet packages"
	@echo "  build    - Build the solution"
	@echo "  rebuild  - Clean and build"
	@echo "  test     - Run tests"
	@echo "  run      - Run the CLI application"
	@echo "  publish  - Publish the application"
	@echo "  format   - Format code"
	@echo "  help     - Show this help"
