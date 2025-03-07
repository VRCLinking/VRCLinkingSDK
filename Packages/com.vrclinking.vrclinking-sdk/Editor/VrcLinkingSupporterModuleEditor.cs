using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRCLinking.Modules.SupporterBoard.Editor
{
    [CustomEditor(typeof(VrcLinkingSupporterModule))]
    public class VrcLinkingSupporterModuleEditor : UnityEditor.Editor
    {

        private SerializedProperty propRoleList;
        private SerializedProperty propSupporterBoardText;
        private SerializedProperty propScrollSpeed;
        private SerializedProperty propScrollWait;
        private SerializedProperty propMaskRect;
        private SerializedProperty propContentRect;

        private ListView RoleListView;
        private VisualElement VariableContainer;
        private VisualElement ReferencesFoldout;


        private void OnEnable()
        {
            propRoleList = serializedObject.FindProperty(nameof(VrcLinkingSupporterModule.roleList));
            propSupporterBoardText = serializedObject.FindProperty(nameof(VrcLinkingSupporterModule.supporterBoardText));
            propScrollSpeed = serializedObject.FindProperty(nameof(VrcLinkingSupporterModule.scrollSpeed));
            propScrollWait = serializedObject.FindProperty(nameof(VrcLinkingSupporterModule.scrollWait));
            propMaskRect = serializedObject.FindProperty(nameof(VrcLinkingSupporterModule.maskRect));
            propContentRect = serializedObject.FindProperty(nameof(VrcLinkingSupporterModule.contentRect));
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();

            VisualTreeAsset uxml = Resources.Load<VisualTreeAsset>(nameof(VrcLinkingSupporterModuleEditor));
            uxml.CloneTree(root);

            RoleListView = root.Q<ListView>(nameof(RoleListView));
            VariableContainer = root.Q<VisualElement>(nameof(VariableContainer));
            ReferencesFoldout = root.Q<Foldout>(nameof(ReferencesFoldout));

            VariableContainer.Add(new PropertyField(propScrollSpeed));
            VariableContainer.Add(new PropertyField(propScrollWait));
            ReferencesFoldout.Add(new PropertyField(propSupporterBoardText));
            ReferencesFoldout.Add(new PropertyField(propMaskRect));
            ReferencesFoldout.Add(new PropertyField(propContentRect));
            
            RoleListView.BindProperty(propRoleList);
            
            return root;
        }
    }
}