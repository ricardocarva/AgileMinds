#!/bin/sh

# Symlink the hooks from the `hooks/` directory to `.git/hooks/`
ln -sf ../../hooks/pre-commit .git/hooks/pre-commit
ln -sf ../../hooks/pre-push .git/hooks/pre-push

# Make sure the hooks are executable
chmod +x .git/hooks/pre-commit
chmod +x .git/hooks/pre-push

echo "Git hooks have been set up successfully!"

