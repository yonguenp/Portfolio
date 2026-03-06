using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class PetSingleDetailInfoPanel : MonoBehaviour
    {
        [SerializeField] PetStatOptions petStatOptions = null;
        [SerializeField] ScrollRect scroll = null;
        bool isShowPanel = false;
        public bool IsShowPanel { get { return isShowPanel; } }

        UserPet currentUserPet = null;


        public int PetTag
        {
            get
            {
                if (currentUserPet == null)
                    return -1;
                return currentUserPet.Tag;
            }
        }
        public void SetVisible(bool _isVisible)
        {
            gameObject.SetActive(_isVisible);
            isShowPanel = _isVisible;

            if (_isVisible && scroll != null)
                scroll.verticalNormalizedPosition = 1;
        }

        public void InitUI(UserPet _petData)
        {
            if (_petData == null)
                return;

            currentUserPet = _petData;
            petStatOptions.CustomPetInfo(_petData);
        }
    }
}
