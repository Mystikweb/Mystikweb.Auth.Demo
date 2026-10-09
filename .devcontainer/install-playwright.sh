#!/bin/bash

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CENTRAL_PACKAGE_FILE="$REPO_ROOT/Directory.Packages.props"
NVM_DIR="${NVM_DIR:-/usr/local/share/nvm}"

if [ -s "$NVM_DIR/nvm.sh" ]; then
    . "$NVM_DIR/nvm.sh"
fi

export PATH="$NVM_DIR/current/bin:$PATH"

PLAYWRIGHT_VERSION=""

if [ -f "$CENTRAL_PACKAGE_FILE" ]; then
    PLAYWRIGHT_VERSION="$(sed -n 's/.*PackageVersion Include="Microsoft\.Playwright" Version="\([^"]*\)".*/\1/p' "$CENTRAL_PACKAGE_FILE" | head -n 1)"
fi

if [ -z "$PLAYWRIGHT_VERSION" ]; then
    PLAYWRIGHT_VERSION="$(sed -n 's/.*PackageReference Include="Microsoft\.Playwright" Version="\([^"]*\)".*/\1/p' "$REPO_ROOT/$PLAYWRIGHT_TEST_PROJECT" | head -n 1)"
fi

if [ -z "$PLAYWRIGHT_VERSION" ]; then
    echo "Unable to determine Microsoft.Playwright version from $CENTRAL_PACKAGE_FILE" >&2
    exit 1
fi

npm install --global "playwright@$PLAYWRIGHT_VERSION" "@playwright/test@$PLAYWRIGHT_VERSION"
playwright install --with-deps