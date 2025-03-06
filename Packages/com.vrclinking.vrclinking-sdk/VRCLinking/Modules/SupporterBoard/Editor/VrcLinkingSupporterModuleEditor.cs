using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;
using System.Collections.Generic;
using VRC.SDK3.Data;

namespace VRCLinking.Modules.SupporterBoard.Editor
{
    [CustomEditor(typeof(VrcLinkingSupporterModule))]
    public class SupporterModuleEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            // Root container
            VisualElement root = new VisualElement();
            root.style.paddingBottom = 10;
            root.style.paddingTop = 10;
            
            // Header Image
            VisualElement header = new VisualElement();
            header.style.backgroundImage = new StyleBackground(Resources.Load("VRCLinking-Banner") as Texture2D);
            header.style.height = 100;
            header.style.width = 400;
            header.style.marginBottom = 10;
            header.style.marginLeft = 10;
            header.style.marginTop = 10;
            root.Add(header);
            
            // Title
            Label titleLabel = new Label("Supporter Module");
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.fontSize = 16;
            titleLabel.style.marginBottom = 10;
            root.Add(titleLabel);
            
            SerializedObject serializedTarget = new SerializedObject(target);
            VrcLinkingSupporterModule supporterModule = (VrcLinkingSupporterModule)target;
            List<SupporterRole> roles = SupporterUtilities.ConvertToSupporterRoles(supporterModule.roles ?? new DataList());
            
            // Assignable fields
            root.Add(CreateObjectField("Supporter Board TMP", serializedTarget.FindProperty("supporterBoardText"), typeof(TextMeshProUGUI)));
            root.Add(CreateAutoApplyFloatField("Scroll Speed", serializedTarget.FindProperty("scrollSpeed")));
            root.Add(CreateAutoApplyFloatField("Scroll Wait", serializedTarget.FindProperty("scrollWait")));
            root.Add(CreateObjectField("Mask Rect", serializedTarget.FindProperty("maskRect"), typeof(RectTransform)));
            root.Add(CreateObjectField("Content Rect", serializedTarget.FindProperty("contentRect"), typeof(RectTransform)));
            
            // Scrollable List of Roles
            ScrollView roleList = new ScrollView();
            foreach (var role in roles)
            {
                roleList.Add(CreateRoleField(role, roles, roleList, supporterModule));
            }
            root.Add(roleList);
            
            // Buttons to modify roles
            Button addRoleButton = new Button(() =>
            {
                SupporterRole newRole = new SupporterRole();
                roles.Add(newRole);
                roleList.Add(CreateRoleField(newRole, roles, roleList, supporterModule));
                ApplyChanges(supporterModule, roles);
            })
            {
                text = "Add Role"
            };
            root.Add(addRoleButton);
            
            return root;
        }

        private VisualElement CreateRoleField(SupporterRole role, List<SupporterRole> roles, ScrollView roleList, VrcLinkingSupporterModule supporterModule)
        {
            VisualElement roleContainer = new VisualElement();
            roleContainer.style.marginBottom = 5;
            roleContainer.style.borderTopWidth = 1;
            roleContainer.style.borderBottomWidth = 1;
            roleContainer.style.borderTopColor = Color.gray;
            roleContainer.style.borderBottomColor = Color.gray;
            
            // Role Type Dropdown
            EnumField roleTypeField = new EnumField("Role Type", role.roleType);
            roleContainer.Add(roleTypeField);
            
            // Container for Role ID/Name and Role Link button
            VisualElement roleInputContainer = new VisualElement();
            roleInputContainer.style.flexDirection = FlexDirection.Row;
            roleContainer.Add(roleInputContainer);
            
            // Role ID/Name
            TextField roleValueField = new TextField("Role ID/Name") { value = role.roleValue };
            roleValueField.style.flexGrow = 2;
            roleInputContainer.Add(roleValueField);
            
            // Button for RoleLink
            Button roleLinkButton = new Button(() => Debug.Log("RoleLink action")) { text = "Configure RoleLink" };
            roleValueField.style.flexGrow = 1;
            roleInputContainer.Add(roleLinkButton);
            
            // Role Color
            ColorField colorField = new ColorField("Role Color") { value = role.roleColor };
            roleContainer.Add(colorField);
            
            // Name Separator
            TextField separatorField = new TextField("Name Separator") { value = role.roleSeparator };
            roleContainer.Add(separatorField);
            
            // Role Relative Size
            FloatField relativeSizeField = new FloatField("Role Relative Size") { value = role.roleRelativeSize };
            roleContainer.Add(relativeSizeField);
            
            // Visibility logic
            void UpdateVisibility()
            {
                bool isRoleLink = role.roleType == RoleType.RoleLink;
                roleLinkButton.style.display = isRoleLink ? DisplayStyle.Flex : DisplayStyle.None;
                roleValueField.isReadOnly = isRoleLink;
            }
            
            roleTypeField.RegisterValueChangedCallback(evt =>
            {
                role.roleType = (RoleType)evt.newValue;
                UpdateVisibility();
                ApplyChanges(supporterModule, roles);
            });
            UpdateVisibility();
            
            // Auto-apply changes for fields
            roleValueField.RegisterValueChangedCallback(evt => { role.roleValue = evt.newValue; ApplyChanges(supporterModule, roles); });
            colorField.RegisterValueChangedCallback(evt => { role.roleColor = evt.newValue; ApplyChanges(supporterModule, roles); });
            separatorField.RegisterValueChangedCallback(evt => { role.roleSeparator = evt.newValue; ApplyChanges(supporterModule, roles); });
            relativeSizeField.RegisterValueChangedCallback(evt => { role.roleRelativeSize = evt.newValue; ApplyChanges(supporterModule, roles); });
            
            // Delete Role Button
            Button deleteButton = new Button(() =>
            {
                roles.Remove(role);
                roleList.Remove(roleContainer);
                ApplyChanges(supporterModule, roles);
            })
            {
                text = "Delete"
            };
            deleteButton.style.marginTop = 5;
            deleteButton.style.backgroundColor = new StyleColor(new Color32(192, 0, 0, 255));
            deleteButton.style.color = Color.white;
            
            roleContainer.Add(deleteButton);
            
            return roleContainer;
        }

        private VisualElement CreateObjectField(string label, SerializedProperty property, System.Type objectType)
        {
            ObjectField field = new ObjectField(label)
            {
                objectType = objectType
            };
            field.BindProperty(property);
            return field;
        }

        private VisualElement CreateAutoApplyFloatField(string label, SerializedProperty property)
        {
            FloatField field = new FloatField(label);
            field.BindProperty(property);
            return field;
        }

        private void ApplyChanges(VrcLinkingSupporterModule supporterModule, List<SupporterRole> roles)
        {
            supporterModule.roles = SupporterUtilities.ConvertToDataList(roles);
            EditorUtility.SetDirty(supporterModule);
        }
    }
}
