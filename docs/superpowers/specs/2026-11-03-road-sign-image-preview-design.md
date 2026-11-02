# Road Sign Image Preview Sizing Design

## Goal

Make the visible Road Sign image preview frame fit the rendered image, matching the Add Question image preview in the Question Bank. Small images should not produce a full-width framed container, while large images must remain responsive within the form.

## Design

Use the same styling pattern as the Question Bank preview:

- Keep the wrapper responsible only for showing, hiding, and positioning the preview.
- Put the padding, border, background, and rounded corners on the preview image itself.
- Render the image at its intrinsic aspect ratio with `width: auto` and `height: auto`.
- Keep `max-width: 100%` and `max-height: 240px` so large images remain within the form and do not dominate the page.
- Leave the existing upload validation and preview-loading behavior unchanged.

The change is limited to the Road Sign preview CSS. Question Bank appearance and behavior will not change.

## Responsive Behavior

For a small image, the visible bordered preview is only as wide as the rendered image plus its padding. For a large image, the image scales down to the available form width or the 240px height limit while preserving its aspect ratio.

## Testing

Add a focused regression check confirming that the Road Sign wrapper no longer owns the visible frame and that its image has the same responsive sizing, padding, border, and radius pattern as the Question Bank preview. Run the regression check and build the web project after implementation.
