using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRCLinking.Modules.SupporterBoard.Editor
{
    [CustomPropertyDrawer(typeof(SupporterRole))]
    public class SupporterRoleDrawer : PropertyDrawer
    {
        // For some reason, having any of these assignments in the root instead of this function results
        // in only the final element having some callbacks occur.
        // To fix this, all assignments and declarations are local to the function.


        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            List<string> dropdownChoices = new List<string>(){"Role 1", "Role 2", "Fake Gamer"};
            List<string> roleChoices = new List<string>(){"ARole 1", "BRole 2", "CFake Gamer"};

            // Find serialized properties.
            SerializedProperty propRoleType = property.FindPropertyRelative(nameof(SupporterRole.roleType));
            SerializedProperty propRoleValue = property.FindPropertyRelative(nameof(SupporterRole.roleValue));
            SerializedProperty propRoleColor = property.FindPropertyRelative(nameof(SupporterRole.roleColor));
            SerializedProperty propRoleSeparator = property.FindPropertyRelative(nameof(SupporterRole.roleSeparator));
            SerializedProperty propRoleRelativeSize =
                property.FindPropertyRelative(nameof(SupporterRole.roleRelativeSize));


            // Create and find elements.
            VisualElement root = new VisualElement();

            VisualTreeAsset uxml = Resources.Load<VisualTreeAsset>("SupporterRoleDrawer");
            uxml.CloneTree(root);

            VisualElement headerColor = root.Q<VisualElement>("HeaderColor");
            Label headerLabel = root.Q<Label>("HeaderLabel");
            EnumField roleTypeField = root.Q<EnumField>("RoleTypeField");
            TextField nameIdField = root.Q<TextField>("NameIdField");
            DropdownField roleSelectionField = root.Q<DropdownField>("RoleSelectionField");
            ColorField roleColorField = root.Q<ColorField>("RoleColorField");
            TextField roleSeparatorField = root.Q<TextField>("SeparatorField");
            FloatField roleSizeField = root.Q<FloatField>("RoleSizeField");


            // Bind elements.
            roleTypeField.BindProperty(propRoleType);
            nameIdField.BindProperty(propRoleValue);
            roleColorField.BindProperty(propRoleColor);
            roleSeparatorField.BindProperty(propRoleSeparator);
            roleSizeField.BindProperty(propRoleRelativeSize);

            roleTypeField.RegisterValueChangedCallback(_ => ValidateRoleField());
            roleSelectionField.choices = dropdownChoices;
            roleSelectionField.RegisterValueChangedCallback(evt => SetRoleValue(dropdownChoices.IndexOf(evt.newValue)));
            roleColorField.RegisterValueChangedCallback(_ => MatchColor());


            MatchColor();
            ValidateRoleField();

            return root;

            void MatchColor()
            {
                Color value = propRoleColor.colorValue;
                value.a = 1;
                headerColor.style.unityBackgroundImageTintColor = value;
                headerLabel.style.color = value;
            }

            void SetRoleValue(int index)
            {
                nameIdField.value = roleChoices[index];
            }
            
            void ValidateRoleField()
            {
                RoleType roleType = (RoleType)propRoleType.enumValueIndex;

                nameIdField.SetEnabled(roleType != RoleType.RoleLink);
                roleSelectionField.style.display =
                    (roleType == RoleType.RoleLink) ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }
}