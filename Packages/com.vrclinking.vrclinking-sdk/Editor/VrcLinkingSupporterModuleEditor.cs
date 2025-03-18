using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRCLinking.Editor;
using VRCLinkingAPI.Model;

namespace VRCLinking.Modules.SupporterBoard.Editor
{
    [CustomEditor(typeof(VrcLinkingSupporterModule))]
    public class VrcLinkingSupporterModuleEditor : UnityEditor.Editor
    {

        private SerializedProperty propSupporterBoardText;
        private SerializedProperty propScrollSpeed;
        private SerializedProperty propScrollWait;
        private SerializedProperty propMaskRect;
        private SerializedProperty propContentRect;

        private SerializedObject serializeHelper;
        private SerializedProperty propRoleList;
        
        private ListView RoleListView;
        private VisualElement VariableContainer;
        private VisualElement ReferencesFoldout;
        VrcLinkingSupporterModuleHelper _helper;

        public static List<string> RoleNames = new List<string>();
        public static List<EncodeRole> Roles = new List<EncodeRole>();


        private void OnEnable()
        {
            propSupporterBoardText = serializedObject.FindProperty(nameof(VrcLinkingSupporterModule.supporterBoardText));
            propScrollSpeed = serializedObject.FindProperty(nameof(VrcLinkingSupporterModule.scrollSpeed));
            propScrollWait = serializedObject.FindProperty(nameof(VrcLinkingSupporterModule.scrollWait));
            propMaskRect = serializedObject.FindProperty(nameof(VrcLinkingSupporterModule.maskRect));
            propContentRect = serializedObject.FindProperty(nameof(VrcLinkingSupporterModule.contentRect));

            _helper = ((VrcLinkingSupporterModule)target).GetComponent<VrcLinkingSupporterModuleHelper>();
            serializeHelper = new SerializedObject(_helper);
            propRoleList = serializeHelper.FindProperty(nameof(VrcLinkingSupporterModuleHelper.roleList));
            
            _ = LoadRoles();
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
            RoleListView.itemsAdded += (evt) =>
            {
                foreach (var item in evt)
                {
                    _helper.roleList[item] = new SupporterRole();
                }
            };
            
            return root;
        }

        private async Task LoadRoles()
        {
            try
            {
                var apiHelper = new VrcLinkingApiHelper();
                var downloader = GetDownloader();
                
                if (downloader == null || downloader.serverId == null)
                {
                    Debug.LogError("VrcLinkingDownloader is not properly set.");
                    return;
                }

                if (! (await apiHelper.IsUserLoggedIn()))
                {
                    Debug.LogError("User is not logged in.");
                    return;
                }

                var roles = await apiHelper.GetAllEncodeRoles(downloader.serverId);
                
                Roles = roles;
                RoleNames.Clear();
                foreach (var role in roles)
                {
                    RoleNames.Add(role.Name);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }
        
        private VrcLinkingDownloader GetDownloader()
        {
            return FindObjectOfType<VrcLinkingDownloader>(true);
        }
    }
}