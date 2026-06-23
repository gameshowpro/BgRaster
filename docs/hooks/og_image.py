def on_page_context(context, page, config, nav):
    if not page.meta.get('og_image'):
        page.meta['og_image'] = 'generated/social-card_0.png'
    if not page.meta.get('og_title'):
        if page.title == config['site_name']:
            page.meta['og_title'] = config['site_name']
        else:
            page.meta['og_title'] = f"{page.title} — {config['site_name']}"