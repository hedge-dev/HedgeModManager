flatpak-builder --force-clean --install-deps-from=flathub --repo=repo builddir app.yml
flatpak build-bundle repo app.flatpak moe.ss16.hedgemodmanager