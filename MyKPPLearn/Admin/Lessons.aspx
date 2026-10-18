<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" ValidateRequest="false" CodeBehind="Lessons.aspx.cs" Inherits="MyKPPLearn.Admin.Lessons" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Lessons | MyKPP Learn</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <asp:Label ID="lblMessage" runat="server" Visible="false" EnableViewState="false" role="status" />

    <!-- ================= LIST ================= -->
    <asp:Panel ID="pnlList" runat="server">
        <div class="page-toolbar">
            <div>
                <h2>Lessons</h2>
                <p>Create and manage driving theory lessons.</p>
            </div>
            <asp:Button ID="btnAddLesson" runat="server" Text="Add Lesson" CssClass="btn btn-primary" CausesValidation="false" OnClick="btnAddLesson_Click" />
        </div>

        <asp:Panel ID="pnlFilters" runat="server" CssClass="filter-bar" DefaultButton="btnSearch">
            <asp:Label runat="server" AssociatedControlID="ddlFilterChapter" CssClass="form-label">Filter by chapter</asp:Label>
            <asp:DropDownList ID="ddlFilterChapter" runat="server" CssClass="form-input form-input--auto" AutoPostBack="true" CausesValidation="false" OnSelectedIndexChanged="ddlFilterChapter_SelectedIndexChanged" />

            <asp:Label runat="server" AssociatedControlID="txtSearch" CssClass="form-label">Search</asp:Label>
            <asp:TextBox ID="txtSearch" runat="server" CssClass="form-input form-input--auto" placeholder="Search lesson titles" MaxLength="100" autocomplete="off" />
            <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-secondary" CausesValidation="false" OnClick="btnSearch_Click" />
            <asp:Button ID="btnClearFilters" runat="server" Text="Clear Filters" CssClass="btn btn-secondary" CausesValidation="false" Visible="false" OnClick="btnClearFilters_Click" />
        </asp:Panel>

        <asp:Panel ID="pnlEmpty" runat="server" CssClass="empty-state" Visible="false">
            <div class="empty-state__title"><asp:Literal ID="litEmptyText" runat="server" Text="No lessons have been created yet." /></div>
            <asp:Button ID="btnEmptyAdd" runat="server" Text="Add Lesson" CssClass="btn btn-primary" CausesValidation="false" OnClick="btnAddLesson_Click" />
            <asp:Button ID="btnEmptyClear" runat="server" Text="Clear Filters" CssClass="btn btn-secondary" CausesValidation="false" Visible="false" OnClick="btnClearFilters_Click" />
        </asp:Panel>

        <asp:Panel ID="pnlTable" runat="server" CssClass="card table-card">
            <div class="table-wrap">
                <table class="data-table">
                    <thead>
                        <tr>
                            <th scope="col">Lesson</th>
                            <th scope="col">Title</th>
                            <th scope="col">Chapter</th>
                            <th scope="col">Status</th>
                            <th scope="col">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptLessons" runat="server" OnItemCommand="rptLessons_ItemCommand" OnItemDataBound="rptLessons_ItemDataBound">
                            <ItemTemplate>
                                <tr class='<%# (bool)Eval("IsActive") ? "" : "row--inactive" %>'>
                                    <td class="data-table__nowrap"><strong><%# MyKPPLearn.Data.LessonRepository.FormatLessonNumber(Eval("ChapterOrder"), Eval("DisplayOrder")) %></strong></td>
                                    <td><%# Server.HtmlEncode(Convert.ToString(Eval("Title"))) %></td>
                                    <td class="data-table__nowrap"><%# Eval("ChapterOrder") %></td>
                                    <td><asp:LinkButton runat="server" ID="btnStatus" CommandName="toggle" CommandArgument='<%# Eval("LessonId") %>' CausesValidation="false" /></td>
                                    <td>
                                        <div class="row-actions">
                                            <asp:LinkButton runat="server" CommandName="view" CommandArgument='<%# Eval("LessonId") %>' CssClass="btn btn-secondary btn-small" CausesValidation="false">View</asp:LinkButton>
                                            <asp:LinkButton runat="server" CommandName="edit" CommandArgument='<%# Eval("LessonId") %>' CssClass="btn btn-secondary btn-small" CausesValidation="false">Edit</asp:LinkButton>
                                            <asp:LinkButton runat="server" CommandName="delete" CommandArgument='<%# Eval("LessonId") %>' CssClass="btn btn-secondary btn-small btn-danger" CausesValidation="false" data-confirm="Are you sure you want to delete this lesson?">Delete</asp:LinkButton>
                                        </div>
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>
        </asp:Panel>
    </asp:Panel>

    <!-- ================= ADD / EDIT ================= -->
    <asp:Panel ID="pnlForm" runat="server" Visible="false">
        <nav class="breadcrumb" aria-label="Breadcrumb">
            <a href="~/Admin/Lessons.aspx" runat="server">Lessons</a>
            <span aria-hidden="true">/</span>
            <span aria-current="page"><asp:Literal ID="litFormCrumb" runat="server" /></span>
        </nav>

        <div class="page-toolbar">
            <h2><asp:Literal ID="litFormHeading" runat="server" /></h2>
        </div>

        <div class="card form-card lesson-form-card">
            <asp:ValidationSummary ID="valSummary" runat="server" CssClass="validation-summary" HeaderText="Please fix the following:" DisplayMode="BulletList" ValidationGroup="LessonForm" />

            <div class="form-group">
                <asp:Label runat="server" AssociatedControlID="ddlChapter" CssClass="form-label">Chapter</asp:Label>
                <asp:DropDownList ID="ddlChapter" runat="server" CssClass="form-input" />
                <asp:RequiredFieldValidator ID="rfvChapter" runat="server" ControlToValidate="ddlChapter" InitialValue="" CssClass="field-error" Display="Dynamic" ErrorMessage="Please select a chapter." ValidationGroup="LessonForm" />
            </div>

            <div class="form-group">
                <asp:Label runat="server" AssociatedControlID="txtLessonNumber" CssClass="form-label">Lesson Number</asp:Label>
                <asp:TextBox ID="txtLessonNumber" runat="server" CssClass="form-input form-input--short" MaxLength="9" />
                <span id="lessonUsedNumbers" class="used-numbers" data-map="<%= UsedNumbersMap %>" data-add="<%= EditingLessonId == 0 ? "1" : "0" %>" hidden></span>
                <asp:RequiredFieldValidator ID="rfvLessonNumber" runat="server" ControlToValidate="txtLessonNumber" CssClass="field-error" Display="Dynamic" ErrorMessage="Lesson number must be a whole number greater than 0." ValidationGroup="LessonForm" />
                <asp:RegularExpressionValidator ID="revLessonNumber" runat="server" ControlToValidate="txtLessonNumber" CssClass="field-error" Display="Dynamic" ValidationExpression="^\s*[1-9][0-9]{0,8}\s*$" ErrorMessage="Lesson number must be a whole number greater than 0." ValidationGroup="LessonForm" />
                <asp:Label ID="lblNumberError" runat="server" CssClass="field-error" Visible="false" EnableViewState="false" role="alert" />
            </div>

            <div class="form-group">
                <asp:Label runat="server" AssociatedControlID="txtTitle" CssClass="form-label">Title</asp:Label>
                <asp:TextBox ID="txtTitle" runat="server" CssClass="form-input" MaxLength="200" />
                <asp:RequiredFieldValidator ID="rfvTitle" runat="server" ControlToValidate="txtTitle" CssClass="field-error" Display="Dynamic" ErrorMessage="Title is required." ValidationGroup="LessonForm" />            </div>

            <div class="form-group">
                <asp:Label runat="server" AssociatedControlID="txtContent" CssClass="form-label">Content</asp:Label>
                <asp:TextBox ID="txtContent" runat="server" CssClass="form-input" TextMode="MultiLine" Rows="15" />
                <span class="form-hint">Plain text. Leave a blank line between paragraphs. Use Ctrl+B (Cmd+B on Mac) to bold selected text.</span>
                <asp:RequiredFieldValidator ID="rfvContent" runat="server" ControlToValidate="txtContent" CssClass="field-error" Display="Dynamic" ErrorMessage="Content is required." ValidationGroup="LessonForm" />
            </div>

            <div class="form-group">
                <asp:Label runat="server" AssociatedControlID="fuImage" CssClass="form-label">Image (optional)</asp:Label>
                <asp:FileUpload ID="fuImage" runat="server" CssClass="form-input" accept=".jpg,.jpeg,.png,.webp" />
                <span class="form-hint">JPG, PNG or WEBP, up to 2 MB.</span>
                <asp:Literal ID="litKeepImageHint" runat="server" />
                <asp:Panel ID="pnlRemoveImage" runat="server" CssClass="remove-image" Visible="false">
                    <label class="remove-image__label"><asp:CheckBox ID="chkRemoveImage" runat="server" /> Remove the current image</label>
                </asp:Panel>
            </div>

            <div class="form-group">
                <span class="form-label">Image Position</span>
                <asp:RadioButtonList ID="rblImagePosition" runat="server" CssClass="radio-list" RepeatDirection="Vertical">
                    <asp:ListItem Text="Above Content" Value="Above" Selected="True" />
                    <asp:ListItem Text="Below Content" Value="Below" />
                </asp:RadioButtonList>
            </div>

            <div class="form-group">
                <span class="form-label">Preview</span>
                <span class="form-hint">How this lesson appears to learners.</span>
                <asp:Panel ID="pnlImagePreview" runat="server" CssClass="image-position-preview">
                    <div class="image-position-preview__head">
                        <span class="badge">Lesson <span id="pvLessonNumber">&mdash;</span></span>
                        <h3 id="pvLessonTitle" class="image-position-preview__title">Lesson title</h3>
                    </div>
                    <div id="previewImageSlot" runat="server" class="image-position-preview__image-slot">
                        <img id="previewImg" runat="server" alt="" />
                    </div>
                    <div id="previewContent" class="image-position-preview__content-slot lesson-content"></div>
                </asp:Panel>
            </div>

            <div class="form-actions">
                <asp:Button ID="btnSave" runat="server" Text="Save Lesson" CssClass="btn btn-primary" ValidationGroup="LessonForm" OnClick="btnSave_Click" />
                <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-secondary" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>
        </div>
    </asp:Panel>

    <!-- ================= VIEW ================= -->
    <asp:Panel ID="pnlView" runat="server" Visible="false">
        <nav class="breadcrumb" aria-label="Breadcrumb">
            <a href="~/Admin/Lessons.aspx" runat="server">Lessons</a>
            <span aria-hidden="true">/</span>
            <span aria-current="page">Lesson Details</span>
        </nav>

        <div class="page-toolbar">
            <h2>Lesson Details</h2>
        </div>

        <%-- Same layout as the learner's lesson page: badge + status, title, image, content, image. --%>
        <article class="card lesson-article">
            <div class="lesson-article__head">
                <span class="badge">Lesson <asp:Literal ID="litViewLesson" runat="server" /></span>
                <asp:Literal ID="litViewStatus" runat="server" />
            </div>
            <h2><asp:Literal ID="litViewTitle" runat="server" /></h2>
            <p class="lesson-article__chapter"><asp:Literal ID="litViewChapter" runat="server" /></p>

            <asp:Panel ID="pnlViewAbove" runat="server" Visible="false" CssClass="lesson-article__image">
                <img id="imgViewAbove" runat="server" class="lesson-image" alt="" />
            </asp:Panel>

            <div class="lesson-content">
                <asp:Literal ID="litViewContent" runat="server" />
            </div>

            <asp:Panel ID="pnlViewBelow" runat="server" Visible="false" CssClass="lesson-article__image">
                <img id="imgViewBelow" runat="server" class="lesson-image" alt="" />
            </asp:Panel>
        </article>

        <div class="form-actions">
            <asp:Button ID="btnViewEdit" runat="server" Text="Edit Lesson" CssClass="btn btn-primary" CausesValidation="false" OnClick="btnViewEdit_Click" />
            <asp:Button ID="btnBack" runat="server" Text="Back to Lessons" CssClass="btn btn-secondary" CausesValidation="false" OnClick="btnCancel_Click" />
        </div>
    </asp:Panel>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script src="<%= ResolveUrl("~/Scripts/used-numbers.js") %>?v=20261010"></script>
    <script>
        // Used Numbers for the CURRENT chapter only; follows the Chapter dropdown (no postback).
        (function () {
            var chapter = document.getElementById("<%= ddlChapter.ClientID %>");
            var used = document.getElementById("lessonUsedNumbers");

            if (!chapter || !used || !window.UsedNumbers) { return; }

            var map = {};
            try { map = JSON.parse(used.getAttribute("data-map") || "{}"); } catch (e) { map = {}; }

            var lesson = document.getElementById("<%= txtLessonNumber.ClientID %>");
            var adding = used.getAttribute("data-add") === "1";
            var lastAuto = adding && lesson ? lesson.value : null;

            function update() {
                used.hidden = !chapter.value;
                used.textContent = chapter.value ? UsedNumbers.format(map[chapter.value] || []) : "";
            }

            // Adding: suggest the next free lesson number for the chosen chapter, unless the admin typed their own.
            function suggestNext() {
                if (!adding || !lesson || !chapter.value) { return; }
                if (lesson.value !== "" && lesson.value !== lastAuto) { return; }

                var numbers = map[chapter.value] || [];
                var max = 0;
                for (var i = 0; i < numbers.length; i++) { if (numbers[i] > max) { max = numbers[i]; } }

                lastAuto = String(max + 1);
                lesson.value = lastAuto;
                lesson.dispatchEvent(new Event("input"));
            }

            chapter.addEventListener("change", function () { update(); suggestNext(); });
            update();
        })();

        (function () {
            var chapter = document.getElementById("<%= ddlChapter.ClientID %>");
            var lesson = document.getElementById("<%= txtLessonNumber.ClientID %>");
            var title = document.getElementById("<%= txtTitle.ClientID %>");
            var previewNumber = document.getElementById("pvLessonNumber");
            var previewTitle = document.getElementById("pvLessonTitle");

            if (!chapter || !lesson || !title || !previewNumber || !previewTitle) { return; }

            // textContent only: typed text is never interpreted as HTML.
            function update() {
                var option = chapter.options[chapter.selectedIndex];
                var chapterOrder = option ? option.getAttribute("data-order") : null;
                var lessonOrder = lesson.value.trim();
                var titleText = title.value.trim();

                previewNumber.textContent = chapterOrder && /^[1-9][0-9]*$/.test(lessonOrder) ? chapterOrder + "." + lessonOrder : "\u2014";
                previewTitle.textContent = titleText || "Lesson title";
            }

            chapter.addEventListener("change", update);
            lesson.addEventListener("input", update);
            title.addEventListener("input", update);
            update();
        })();

        (function () {
            var fileInput = document.getElementById("<%= fuImage.ClientID %>");
            var previewImg = document.getElementById("<%= previewImg.ClientID %>");
            var previewImageSlot = document.getElementById("<%= previewImageSlot.ClientID %>");
            var previewBox = document.getElementById("<%= pnlImagePreview.ClientID %>");
            var positionInputs = document.querySelectorAll("input[name='<%= rblImagePosition.UniqueID %>']");

            if (!fileInput || !previewImg || !previewImageSlot || !previewBox) { return; }

            var objectUrl = null;

            function showImage(src) {
                previewImg.src = src;
                previewImageSlot.style.display = "block";
            }

            fileInput.addEventListener("change", function () {
                var file = fileInput.files && fileInput.files[0];

                if (objectUrl) {
                    URL.revokeObjectURL(objectUrl);
                    objectUrl = null;
                }

                if (!file) { return; }

                objectUrl = URL.createObjectURL(file);
                showImage(objectUrl);

                // Choosing a new file replaces the current image, so "Remove" no longer applies.
                if (removeBox) { removeBox.checked = false; }
            });

            // "Remove the current image": hide it from the preview (only if no new file was chosen).
            var removeBox = document.getElementById("<%= chkRemoveImage.ClientID %>");

            if (removeBox) {
                removeBox.addEventListener("change", function () {
                    if (removeBox.checked && !(fileInput.files && fileInput.files[0])) {
                        previewImageSlot.style.display = "none";
                    } else if (!removeBox.checked && previewImg.getAttribute("src")) {
                        previewImageSlot.style.display = "block";
                    }
                });
            }

            function updatePosition() {
                var checked = null;

                for (var i = 0; i < positionInputs.length; i++) {
                    if (positionInputs[i].checked) { checked = positionInputs[i].value; }
                }

                previewBox.classList.toggle("image-position-preview--below", checked === "Below");
            }

            for (var j = 0; j < positionInputs.length; j++) {
                positionInputs[j].addEventListener("change", updatePosition);
            }

            updatePosition();
        })();

        (function () {
            var textarea = document.getElementById("<%= txtContent.ClientID %>");
            var preview = document.getElementById("previewContent");

            if (!textarea || !preview) { return; }

            // Must mirror MyKPPLearn.Common.LearningText.ToSafeHtml: HTML-encode first, then apply
            // only the controlled **bold** replacement. Never assign raw textarea.value to innerHTML.
            function escapeHtml(value) {
                return value
                    .replace(/&/g, "&amp;")
                    .replace(/</g, "&lt;")
                    .replace(/>/g, "&gt;")
                    .replace(/"/g, "&quot;")
                    .replace(/'/g, "&#39;");
            }

            function applyBold(encoded) {
                return encoded.replace(/\*\*([\s\S]+?)\*\*/g, "<strong>$1</strong>");
            }

            function buildPreviewHtml(raw) {
                var normalized = raw.replace(/\r\n/g, "\n").replace(/\r/g, "\n");
                var paragraphs = normalized.split(/\n{2,}/);
                var html = "";

                for (var i = 0; i < paragraphs.length; i++) {
                    var trimmed = paragraphs[i].replace(/^[\n \t]+|[\n \t]+$/g, "");

                    if (!trimmed) { continue; }

                    html += "<p>" + applyBold(escapeHtml(trimmed)).replace(/\n/g, "<br />") + "</p>";
                }

                return html;
            }

            function updatePreview() {
                var value = textarea.value || "";

                if (!value.trim()) {
                    preview.innerHTML = "";
                    var empty = document.createElement("span");
                    empty.className = "content-preview__empty";
                    empty.textContent = "Your formatted lesson content will appear here.";
                    preview.appendChild(empty);
                    return;
                }

                preview.innerHTML = buildPreviewHtml(value);
            }

            function applyBoldToSelection() {
                var start = textarea.selectionStart;
                var end = textarea.selectionEnd;
                var value = textarea.value;
                var selected = value.substring(start, end);

                if (selected.length === 0) {
                    textarea.value = value.substring(0, start) + "****" + value.substring(start);
                    textarea.focus();
                    textarea.setSelectionRange(start + 2, start + 2);
                } else if (value.substring(start - 2, start) === "**" && value.substring(end, end + 2) === "**") {
                    // Already bold: remove the surrounding markers instead of nesting new ones.
                    textarea.value = value.substring(0, start - 2) + selected + value.substring(end + 2);
                    textarea.focus();
                    textarea.setSelectionRange(start - 2, end - 2);
                } else {
                    textarea.value = value.substring(0, start) + "**" + selected + "**" + value.substring(end);
                    textarea.focus();
                    textarea.setSelectionRange(start + 2, end + 2);
                }

                updatePreview();
            }

            // Scoped to this textarea only: Ctrl+B / Cmd+B elsewhere on the page (Title, Lesson
            // number, Chapter) is left alone.
            textarea.addEventListener("keydown", function (event) {
                var isBold = (event.ctrlKey || event.metaKey) && event.key.toLowerCase() === "b";

                if (!isBold) { return; }

                event.preventDefault();
                applyBoldToSelection();
            });

            textarea.addEventListener("input", updatePreview);
            updatePreview();
        })();
    </script>
</asp:Content>
