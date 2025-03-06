using System.Collections.Generic;
using System.Text;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.Udon.Serialization.OdinSerializer;
using VRCLinking.Modules;
using VRCLinking.Modules.SupporterBoard.Editor;
using VRCLinking.Utilitites;

namespace VRCLinking.Modules.SupporterBoard
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VrcLinkingSupporterModule : VrcLinkingModuleBase
    {
        public TextMeshProUGUI supporterBoardText;
        
        [OdinSerialize]
        public DataList roles;
        
        [SerializeField] internal float scrollSpeed = 1f;
        [SerializeField] internal float scrollWait = 3f;
        
        [SerializeField] internal RectTransform maskRect;
        [SerializeField] internal RectTransform contentRect;
        
        private protected override string ModuleName => "VrcLinkingSupporterModule";
        

        public override void OnDataLoaded()
        {
            if (supporterBoardText == null)
            {
                Log("Supporter Board Text is not set.");
                return;
            }

            var sb = new StringBuilder();
            for (var ir = 0; ir < roles.Count; ir++)
            {
                var role = roles[ir].DataDictionary;
                var roleType = (RoleType)role["roleType"].Int;
                var rank = role["roleValue"].String;
                var nameSeparator = role["roleSeparator"].String;
                var roleRelativeSize = role["roleRelativeSize"].Float;
                
                sb.Append($"<color={((Color)role["roleColor"].Reference).ToHex()}>");
                sb.Append($"<size={roleRelativeSize}%>");

                if (roleType == RoleType.RoleId)
                {
                    if (downloader.TryGetGuildMembersByRoleId(rank, out DataList members))
                    {
                        for (int i = 0; i < members.Count; i++)
                        {
                            sb.Append(members[i].String);
                            if (i < members.Count - 1)
                            {
                                sb.Append(nameSeparator);
                            }
                        }
                    }
                }
                else if (roleType == RoleType.RoleName)
                {
                    if (downloader.TryGetGuildMembersByRoleName(rank, out DataList members))
                    {
                        for (int i = 0; i < members.Count; i++)
                        {
                            sb.Append(members[i].String);
                            if (i < members.Count - 1)
                            {
                                sb.Append(nameSeparator);
                            }
                        }
                    }
                }
                

                
                sb.Append("</color>");
                sb.Append("</size>");

                sb.AppendLine();
            }
            
            supporterBoardText.text = sb.ToString();
            ResetScroll();
            SendCustomEventDelayedSeconds(nameof(_CustomUpdate), scrollWait);
        }
        
        void ResetScroll()
        {
            if (maskRect == null || contentRect == null)
            {
                return;
            }
            
            contentRect.anchoredPosition = new Vector2(0, 0);
        }
        
        void ReverseScroll()
        {
            if (maskRect == null || contentRect == null)
            {
                return;
            }
            
            scrollSpeed = -scrollSpeed;
        }

        float _waitTime = 0;
        public void _CustomUpdate()
        {
            if (maskRect == null || contentRect == null)
            {
                return;
            }

            if (contentRect.rect.height <= maskRect.rect.height)
            {
                return;
            }

            if (contentRect.anchoredPosition.y > contentRect.rect.height - maskRect.rect.height)
            {
                _waitTime += Time.deltaTime;
                if (_waitTime >= scrollWait)
                {
                    ReverseScroll();
                    contentRect.anchoredPosition = new Vector2(0, contentRect.rect.height - maskRect.rect.height);
                    _waitTime = 0;
                }
                SendCustomEventDelayedFrames(nameof(_CustomUpdate), 1);
                return;
            }

            if (contentRect.anchoredPosition.y < 0)
            {
                _waitTime += Time.deltaTime;
                if (_waitTime >= scrollWait)
                {
                    ReverseScroll();
                    contentRect.anchoredPosition = new Vector2(0, 0);
                    _waitTime = 0;
                }
                SendCustomEventDelayedFrames(nameof(_CustomUpdate), 1);
                return;
            }

            contentRect.anchoredPosition += new Vector2(0, scrollSpeed * Time.deltaTime);
            
            
            SendCustomEventDelayedFrames(nameof(_CustomUpdate), 1);
        }
        
        
    }
}
