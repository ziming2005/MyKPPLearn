# Road Sign Game Question Options Design

## Goal

Make the Road Sign Game **Edit Question** options match the Question Bank **Add Question** options: three stacked A/B/C rows with an inline **Correct** radio on every row.

## Scope

- Replace the Road Sign Game's three-column option inputs and separate **Correct Answer** radio group with one full-width **Options** group.
- Each row contains the existing option input between a circular letter marker and its corresponding **Correct** radio.
- Reuse the Question Bank's existing `.qb-option`, `.qb-option__letter`, and `.qb-option__correct` presentation classes so both interfaces remain visually consistent and share responsive behavior.
- Do not add instructional helper text or change the surrounding Question, Selected Road Sign, or Explanation fields.

## Behavior and Compatibility

- Preserve `hrsOptionA`, `hrsOptionB`, and `hrsOptionC` input IDs.
- Preserve `hrsCorrect1`, `hrsCorrect2`, and `hrsCorrect3` radio IDs, the `hrsCorrect` radio group name, and values `1`, `2`, and `3`.
- Preserve the current `data-field` attributes and disabled states used by the Road Sign Game editor JavaScript.
- No data model, server-side, validation, or save-flow changes are required.

## Responsive and Accessible Design

- Use the existing full-width form-grid modifier for the Options group.
- Retain an accessible group label and radio-group relationship.
- Keep explicit labels for every radio and accessible labels for the option inputs.
- On narrow screens, use the Question Bank's existing wrapping behavior so the **Correct** control moves below its input without horizontal overflow.

## Verification

- Confirm all three option rows match the Question Bank layout at desktop and mobile widths.
- Confirm selecting a road sign loads its option text and selected correct answer.
- Confirm editing and saving preserves the three options and correct-answer value.
- Confirm disabled controls remain disabled when no road sign is selected.
