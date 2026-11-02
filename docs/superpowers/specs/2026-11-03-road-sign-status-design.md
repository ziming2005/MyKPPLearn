# Road Sign Status Form Removal Design

## Goal

Remove the Status option from both the Add Road Sign and Edit Road Sign forms. Every newly created Road Sign must start Active. Editing an existing Road Sign must preserve its current status. Administrators will continue to activate or deactivate Road Signs using the status button in the Road Sign list.

## Scope

- Remove the Status radio-button group and its explanatory hint from the shared Road Sign form.
- Remove the obsolete server-control declaration and form initialization code.
- Make the create path pass `true` explicitly for the new Road Sign's active state.
- Make the edit path read and preserve the existing Road Sign's `IsActive` value.
- Keep the existing Active/Inactive list button and its behavior unchanged.

No database schema, repository contract, filtering, deletion, or Road Sign Game behavior will change.

## Data Flow

### Create

1. The administrator enters a name and selects an image.
2. Server-side validation runs as it does today.
3. The page calls the repository create operation with `isActive: true`.
4. The new Road Sign appears as Active in the list.

### Edit

1. The administrator opens an existing Road Sign and changes its name or image.
2. The page reloads the existing database row during save.
3. The page passes the row's current `IsActive` value to the repository update operation.
4. The Road Sign retains its previous status.

### Status Changes

The administrator uses the existing status button in the Road Sign list. That remains the only UI for changing a Road Sign's status.

## Error Handling

Existing validation and error messages remain unchanged. If an edited Road Sign no longer exists, the current error path returns to the list before an update is attempted. Because status comes from the database rather than submitted form data, a missing or manipulated Status form value cannot affect creation or editing.

## Testing

Add a focused regression test that verifies:

- the shared Add/Edit form no longer contains the Status radio-button control or hint;
- the create path supplies `true` to the repository;
- the edit path supplies the existing row's `IsActive` value to the repository; and
- the Road Sign list still contains its status action.

Run the regression test and build the web project after implementation.
