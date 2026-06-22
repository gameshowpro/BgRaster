# Set site-wide og:image default if page frontmatter doesn't specify one.
def on_page_context(context, page, config, nav):
    if not page.meta.get('og_image'):
        page.meta['og_image'] = 'generated/slices.png'