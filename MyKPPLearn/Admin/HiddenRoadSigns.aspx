<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" ValidateRequest="false" CodeBehind="HiddenRoadSigns.aspx.cs" Inherits="MyKPPLearn.Admin.HiddenRoadSigns" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Road Sign Game | MyKPP Learn</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <asp:Label ID="lblMessage" runat="server" Visible="false" EnableViewState="false" role="status" />

    <!-- ================= LIST ================= -->
    <asp:Panel ID="pnlList" runat="server">
        <div class="page-toolbar">
            <div>
                <h2>Road Sign Game</h2>
                <p>Each Set has one Scene Image and several hidden road signs placed in the scene.</p>
            </div>
            <asp:Button ID="btnAddSet" runat="server" Text="Add Game Set" CssClass="btn btn-primary" CausesValidation="false" OnClick="btnAddSet_Click" />
        </div>

        <asp:Panel ID="pnlEmpty" runat="server" CssClass="empty-state" Visible="false">
            <div class="empty-state__title">No Road Sign Game sets yet.</div>
            <asp:Button ID="btnEmptyAdd" runat="server" Text="Add Game Set" CssClass="btn btn-primary" CausesValidation="false" OnClick="btnAddSet_Click" />
        </asp:Panel>

        <asp:Panel ID="pnlTable" runat="server" CssClass="card table-card">
            <div class="table-wrap">
                <table class="data-table">
                    <thead>
                        <tr>
                            <th scope="col">Set</th>
                            <th scope="col">Road Signs</th>
                            <th scope="col">Attempts</th>
                            <th scope="col">Status</th>
                            <th scope="col">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptSets" runat="server" OnItemCommand="rptSets_ItemCommand" OnItemDataBound="rptSets_ItemDataBound">
                            <ItemTemplate>
                                <tr class='<%# (bool)Eval("IsActive") ? "" : "row--inactive" %>'>
                                    <td class="data-table__nowrap"><strong><%# Eval("SetNumber") %></strong></td>
                                    <td class="data-table__nowrap"><span class='<%# SignsBadgeClass(Eval("ConfiguredCount"), Eval("ItemCount")) %>' title='<%# SignsBadgeTitle(Eval("ConfiguredCount"), Eval("ItemCount")) %>'><%# Eval("ConfiguredCount") %> / <%# Eval("ItemCount") %> configured</span></td>
                                    <td class="data-table__nowrap"><%# Eval("AttemptCount") %></td>
                                    <td><asp:LinkButton runat="server" ID="btnStatus" CommandName="toggle" CommandArgument='<%# Eval("HiddenRoadSignPracticeId") %>' CausesValidation="false" /></td>
                                    <td>
                                        <div class="row-actions">
                                            <asp:LinkButton runat="server" CommandName="view" CommandArgument='<%# Eval("HiddenRoadSignPracticeId") %>' CssClass="btn btn-secondary btn-small" CausesValidation="false">View</asp:LinkButton>
                                            <asp:LinkButton runat="server" CommandName="edit" CommandArgument='<%# Eval("HiddenRoadSignPracticeId") %>' CssClass="btn btn-secondary btn-small" CausesValidation="false">Edit</asp:LinkButton>
                                            <asp:LinkButton runat="server" ID="btnDelete" CommandName="delete" CommandArgument='<%# Eval("HiddenRoadSignPracticeId") %>' CssClass="btn btn-secondary btn-small btn-danger" CausesValidation="false" data-confirm="Are you sure you want to delete this Road Sign Game set?">Delete</asp:LinkButton>
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

    <!-- ================= ADD / EDIT (one page-state editor, ONE final Save) ================= -->
    <asp:Panel ID="pnlForm" runat="server" Visible="false">
        <nav class="breadcrumb" aria-label="Breadcrumb">
            <a href="~/Admin/HiddenRoadSigns.aspx" runat="server">Road Sign Game</a>
            <span aria-hidden="true">/</span>
            <span aria-current="page"><asp:Literal ID="litFormCrumb" runat="server" /></span>
        </nav>

        <div class="page-toolbar">
            <h2><asp:Literal ID="litFormHeading" runat="server" /></h2>
        </div>

        <div class="card form-card hrs-details">
            <h3 class="hrs-section-title">Game Set Details</h3>

            <asp:ValidationSummary ID="valSummary" runat="server" CssClass="validation-summary" HeaderText="Please fix the following:" DisplayMode="BulletList" ValidationGroup="SetForm" />

            <div class="form-group">
                <asp:Label runat="server" AssociatedControlID="txtSetNumber" CssClass="form-label">Game Set Number</asp:Label>
                <asp:TextBox ID="txtSetNumber" runat="server" CssClass="form-input form-input--short" MaxLength="9" />
                <span class="used-numbers"><%= UsedNumbersText %></span>
                <span class="form-hint">Enter the set number only, for example 1 (shown to learners as &ldquo;Road Sign Game Set 1&rdquo;). Each number must be unique.</span>
                <asp:Label ID="lblNumberError" runat="server" CssClass="field-error" Visible="false" EnableViewState="false" role="alert" />
                <asp:RequiredFieldValidator ID="rfvSetNumber" runat="server" ControlToValidate="txtSetNumber" CssClass="field-error" Display="Dynamic" ErrorMessage="Set number must be a whole number greater than 0." ValidationGroup="SetForm" />
                <asp:RegularExpressionValidator ID="revSetNumber" runat="server" ControlToValidate="txtSetNumber" CssClass="field-error" Display="Dynamic" ValidationExpression="^\s*[1-9][0-9]{0,8}\s*$" ErrorMessage="Set number must be a whole number greater than 0." ValidationGroup="SetForm" />
            </div>

            <div class="form-group">
                <span class="form-label">Status</span>
                <asp:RadioButtonList ID="rblStatus" runat="server" CssClass="radio-list" RepeatDirection="Vertical">
                    <asp:ListItem Text="Active" Value="Active" />
                    <asp:ListItem Text="Inactive" Value="Inactive" Selected="True" />
                </asp:RadioButtonList>
                <span class="form-hint">A set can only be Active once it has at least one road sign and every road sign question is complete. New sets start as Inactive.</span>
            </div>

            <div class="form-group">
                <asp:Label runat="server" AssociatedControlID="fuScene" CssClass="form-label">Scene Image</asp:Label>
                <asp:FileUpload ID="fuScene" runat="server" CssClass="form-input" accept=".jpg,.jpeg,.png,.webp" />
                <span class="form-hint">JPG, PNG or WEBP, up to 4 MB.<asp:Literal ID="litKeepImageHint" runat="server" /></span>
            </div>
        </div>

        <!-- ================= MANAGE ROAD SIGNS ================= -->
        <div class="hrs-manage">
            <div class="hrs-manage__head">
                <h3 class="hrs-section-title">Manage Road Signs</h3>
                <p>Drag road signs into the scene, then select each one to configure its question.</p>
            </div>

            <div id="hrsNotice" class="alert" role="status" hidden></div>

            <div id="hrsEditor" runat="server" class="hrs-editor">
                <input type="hidden" runat="server" id="hfState" />

                <p class="hrs-rotate-hint" role="note">
                    <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false"><rect x="2" y="7" width="20" height="10" rx="2"/><path d="M12 17h.01"/></svg>
                    Rotate your phone to landscape for a bigger scene.
                </p>

                <p id="hrsNoScene" class="card hrs-manage__locked">Upload a Scene Image to start placing road signs.</p>

                <div id="hrsEditorBody" class="hrs-editor__body" hidden>

                    <div class="hrs-editor__top">

                        <!-- Scene Editor: the Scene Image itself (also the live preview of a newly chosen file) -->
                        <div class="card hrs-editor__scene">
                            <div id="hrsStage" class="hrs-stage">
                                <img id="imgScene" runat="server" alt="Scene Image" draggable="false" />
                            </div>
                        </div>

                        <!-- Road Sign Library -->
                        <aside class="card hrs-library" aria-labelledby="hrsLibraryTitle">
                            <div class="hrs-library__inner">
                                <h3 id="hrsLibraryTitle">Road Sign Library</h3>

                                <asp:Panel ID="pnlNoAssets" runat="server" CssClass="hrs-library__empty" Visible="false">
                                    <p>No active Road Sign Assets available.</p>
                                    <p class="form-hint">You can manage them from <a href="~/Admin/RoadSignAssets.aspx" runat="server">Road Sign Assets</a>.</p>
                                </asp:Panel>

                                <div id="hrsLibraryBody" runat="server" class="hrs-library__body">
                                    <input type="search" id="hrsSearch" class="form-input" placeholder="Search road signs..." aria-label="Search road signs" autocomplete="off" />
                                    <p class="form-hint hrs-library__tip">Drag a sign onto the scene.</p>
                                    <div id="hrsLibrary" class="hrs-library__grid" role="list"></div>
                                    <p id="hrsLibraryEmpty" class="hrs-library__nomatch" hidden>No road signs found.</p>
                                </div>
                            </div>
                        </aside>
                    </div>

                    <!-- Edit Question (left) + Scene Road Signs (right), full width. No save button here: the whole Game Set is saved once, below. -->
                    <div class="hrs-bottom">

                        <div class="card hrs-form">
                            <h3 id="hrsFormTitle">Edit Question</h3>

                            <div class="hrs-form__grid">

                                <div class="form-group hrs-form__wide">
                                    <span class="form-label">Selected Road Sign</span>
                                    <div class="hrs-selected">
                                        <span class="hrs-selected__thumb"><img id="hrsSelectedImg" alt="" hidden /></span>
                                        <span class="hrs-selected__text">
                                            <span id="hrsSelectedName" class="hrs-selected__name">None selected</span>
                                            <span id="hrsSelectedStatus" class="hrs-status" hidden></span>
                                        </span>
                                        <button type="button" id="hrsRemoveBtn" class="btn btn-secondary btn-small btn-danger hrs-selected__remove" hidden>Remove from Scene</button>
                                    </div>
                                </div>

                                <div class="form-group hrs-form__wide">
                                    <label class="form-label" for="hrsQuestion">Question</label>
                                    <textarea id="hrsQuestion" class="form-input" rows="3" maxlength="500" data-field="question" disabled></textarea>
                                    <span class="char-counter"><span id="hrsQuestionCount">0</span> / 500</span>
                                </div>

                                <div class="form-group">
                                    <label class="form-label" for="hrsOptionA">Option A</label>
                                    <input type="text" id="hrsOptionA" class="form-input" maxlength="200" data-field="a" disabled />
                                </div>

                                <div class="form-group">
                                    <label class="form-label" for="hrsOptionB">Option B</label>
                                    <input type="text" id="hrsOptionB" class="form-input" maxlength="200" data-field="b" disabled />
                                </div>

                                <div class="form-group">
                                    <label class="form-label" for="hrsOptionC">Option C</label>
                                    <input type="text" id="hrsOptionC" class="form-input" maxlength="200" data-field="c" disabled />
                                </div>

                                <div class="form-group hrs-form__wide">
                                    <span class="form-label" id="lblCorrect">Correct Answer</span>
                                    <div data-field="correct" role="radiogroup" aria-labelledby="lblCorrect" class="radio-list radio-list--inline">
                                        <span><input type="radio" id="hrsCorrect1" name="hrsCorrect" value="1" disabled /><label for="hrsCorrect1">A</label></span>
                                        <span><input type="radio" id="hrsCorrect2" name="hrsCorrect" value="2" disabled /><label for="hrsCorrect2">B</label></span>
                                        <span><input type="radio" id="hrsCorrect3" name="hrsCorrect" value="3" disabled /><label for="hrsCorrect3">C</label></span>
                                    </div>
                                </div>

                                <div class="form-group hrs-form__wide">
                                    <label class="form-label" for="hrsExplanation">Explanation</label>
                                    <textarea id="hrsExplanation" class="form-input" rows="3" maxlength="500" data-field="explanation" disabled></textarea>
                                    <span class="char-counter"><span id="hrsExplanationCount">0</span> / 500</span>
                                    <span class="form-hint">Required. Shown to the learner only after an incorrect answer.</span>
                                </div>

                                <p id="hrsFormProblem" class="field-error hrs-form__wide" role="alert" hidden></p>
                            </div>
                        </div>

                        <aside class="card hrs-scenelist" aria-labelledby="hrsSceneListTitle">
                            <h3 id="hrsSceneListTitle">Scene Road Signs</h3>
                            <p id="hrsSceneListEmpty" class="form-hint" hidden>No road signs placed yet.</p>
                            <ul id="hrsList" class="hrs-scenelist__list"></ul>
                        </aside>
                    </div>
                </div>
            </div>
        </div>

        <%-- Shown when the final Save is attempted while road signs are incomplete. --%>
        <div id="hrsIncompleteModal" class="modal-overlay" hidden>
            <div class="modal-panel card hrs-modal" role="alertdialog" aria-modal="true" aria-labelledby="hrsIncompleteTitle" aria-describedby="hrsIncompleteText">
                <h3 id="hrsIncompleteTitle">Incomplete road sign questions</h3>
                <p id="hrsIncompleteText"></p>
                <div class="form-actions">
                    <button type="button" id="hrsIncompleteOk" class="btn btn-primary">Review questions</button>
                </div>
            </div>
        </div>

        <!-- ================= THE ONE SAVE ================= -->
        <asp:Label ID="lblSaveError" runat="server" Visible="false" EnableViewState="false" role="alert" />

        <div class="form-actions hrs-final-actions">
            <asp:Button ID="btnSave" runat="server" Text="Save Game Set" CssClass="btn btn-primary" ValidationGroup="SetForm" OnClientClick="return window.hrsEditor ? window.hrsEditor.beforeSave() : true;" OnClick="btnSave_Click" />
            <asp:Button ID="btnCancel" runat="server" Text="Back to Game Sets" CssClass="btn btn-secondary" CausesValidation="false" OnClientClick="return window.hrsEditor ? window.hrsEditor.confirmLeave() : true;" OnClick="btnCancel_Click" />
        </div>
    </asp:Panel>

    <!-- ================= VIEW (read-only learner preview) ================= -->
    <asp:Panel ID="pnlView" runat="server" Visible="false">
        <nav class="breadcrumb" aria-label="Breadcrumb">
            <a href="~/Admin/HiddenRoadSigns.aspx" runat="server">Road Sign Game</a>
            <span aria-hidden="true">/</span>
            <span aria-current="page">Game Set Details</span>
        </nav>

        <div class="page-toolbar">
            <h2>Game Set Details</h2>
        </div>

        <%-- Same layout as the learner pages: badge + status, title, then the scene the learner sees. --%>
        <article class="card lesson-article">
            <div class="lesson-article__head">
                <span class="badge">Road Sign Game</span>
                <asp:Literal ID="litViewStatus" runat="server" />
            </div>
            <h2><asp:Literal ID="litViewSet" runat="server" /></h2>
            <p class="lesson-article__chapter"><asp:Literal ID="litViewItems" runat="server" /> &middot; <asp:Literal ID="litViewAttempts" runat="server" /></p>

            <h3 class="hrs-section-title">Learner Scene Preview</h3>
            <%-- Read-only: the Scene Image plus the CONFIGURED road signs only, exactly as a learner first sees it. --%>
            <div class="hrs-view-scene">
                <img id="imgView" runat="server" alt="" />
                <div class="hrs-view-signs" aria-hidden="true"><asp:Literal ID="litViewSigns" runat="server" /></div>
            </div>

            <h3 class="hrs-section-title hrs-view-questions-title">Road Sign Questions</h3>
            <asp:Literal ID="litViewQuestions" runat="server" />
        </article>

        <div class="form-actions">
            <asp:Button ID="btnViewEdit" runat="server" Text="Edit Game Set" CssClass="btn btn-primary" CausesValidation="false" OnClick="btnViewEdit_Click" />
            <asp:Button ID="btnBack" runat="server" Text="Back to Game Sets" CssClass="btn btn-secondary" CausesValidation="false" OnClick="btnCancel_Click" />
        </div>
    </asp:Panel>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script src="<%= ResolveUrl("~/Scripts/road-sign-fit.js") %>?v=20261010"></script>
    <script src="<%= ResolveUrl("~/Scripts/hidden-road-sign-view.js") %>?v=20261010"></script>
    <script src="<%= ResolveUrl("~/Scripts/hidden-road-sign-editor.js") + "?v=" + EditorScriptVersion %>"></script>
</asp:Content>
