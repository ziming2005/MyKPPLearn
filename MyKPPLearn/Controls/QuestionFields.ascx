<%@ Control Language="C#" %>
<%--
    The fields of one question: text, optional image, options A-C with the correct answer,
    and explanation. Shared by Admin/QuestionBank.aspx and Admin/PracticeSets.aspx (one per page, so the ids are
    plain HTML ids); Scripts/practice-admin-db.js (createQuestionFields) fills, validates and reads them.
--%>

<div class="form-group">
    <label class="form-label" for="qbFormText">Question</label>
    <textarea id="qbFormText" class="form-input" rows="3" maxlength="500"></textarea>
    <span class="char-counter"><span id="qbTextCount">0</span> / 500</span>
    <p id="qbTextError" class="field-error" role="alert" hidden></p>
</div>

<div class="form-group">
    <label class="form-label" for="qbFormImage">Image (optional)</label>
    <input type="file" id="qbFormImage" class="form-input" accept=".jpg,.jpeg,.png,.webp" />
    <span class="form-hint">JPG, PNG or WEBP, up to 10 MB.</span>
    <p id="qbImageError" class="field-error" role="alert" hidden></p>

    <div id="qbImagePreviewWrap" class="qb-image-preview" hidden>
        <img id="qbImagePreview" alt="" />
    </div>
    <div id="qbImageTools" class="qb-image-tools" hidden>
        <button type="button" id="qbRemoveImage" class="btn btn-secondary btn-small btn-danger">Remove image</button>
    </div>
</div>
<div class="form-group">
    <span class="form-label" id="qbOptionsLabel">Options</span>
    <span class="form-hint">Enter three different options (A, B and C) and choose the correct one.</span>

    <div class="qb-option">
        <span class="qb-option__letter" aria-hidden="true">A</span>
        <input id="qbFormOptionA" type="text" class="form-input" maxlength="200" aria-label="Option A" autocomplete="off" />
        <label class="qb-option__correct"><input type="radio" name="qbCorrectAnswer" value="0" /> Correct</label>
    </div>
    <div class="qb-option">
        <span class="qb-option__letter" aria-hidden="true">B</span>
        <input id="qbFormOptionB" type="text" class="form-input" maxlength="200" aria-label="Option B" autocomplete="off" />
        <label class="qb-option__correct"><input type="radio" name="qbCorrectAnswer" value="1" /> Correct</label>
    </div>
    <div class="qb-option">
        <span class="qb-option__letter" aria-hidden="true">C</span>
        <input id="qbFormOptionC" type="text" class="form-input" maxlength="200" aria-label="Option C" autocomplete="off" />
        <label class="qb-option__correct"><input type="radio" name="qbCorrectAnswer" value="2" /> Correct</label>
    </div>

    <p id="qbOptionsError" class="field-error" role="alert" hidden></p>
    <p id="qbCorrectError" class="field-error" role="alert" hidden></p>
</div>

<div class="form-group">
    <label class="form-label" for="qbFormExplanation">Explanation</label>
    <textarea id="qbFormExplanation" class="form-input" rows="3" maxlength="500"></textarea>
    <span class="char-counter"><span id="qbExplanationCount">0</span> / 500</span>
    <p id="qbExplanationError" class="field-error" role="alert" hidden></p>
</div>
