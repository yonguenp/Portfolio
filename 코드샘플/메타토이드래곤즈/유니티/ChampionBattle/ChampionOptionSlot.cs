using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Google.Impl;
using System;

namespace SandboxNetwork
{
    public class ChampionOptionSlot : MonoBehaviour
    {
        [SerializeField]
        private GameObject optionPrefab = null;
        [SerializeField]
        private Transform optionParent = null;


        private ChampionOptionSelectSlot selectedOption = null;
        Color selectedBackColor = new Color(56f / 255f, 70f / 255f, 116f / 255f);
        public int curLayer = 0;

        public delegate void func(int layer, int row);
        private func clickCallback = null;

        OptionSelectPopup parent = null;

        void Clear()
        {
            foreach (Transform child in optionParent)
            {
                if (child == optionPrefab.transform)
                    continue;

                Destroy(child.gameObject);
            }
        }
        public void Init(List<PetStatData> _optionList, int layer, OptionSelectPopup p, int selected = -1)
        {
            Clear();

            parent = p;
            curLayer = layer;
            selectedOption = null;

            optionPrefab.SetActive(true);
            foreach (var data in _optionList)
            {
                var clone = Instantiate(optionPrefab, optionParent);
                var slot = clone.GetComponent<ChampionOptionSelectSlot>();
                slot.SetData(data, this);
                if(data.KEY == selected)
                {
                    selectedOption = slot;
                }
            }

            setColors(selectedOption);
            optionPrefab.SetActive(false);

            
            LayoutRebuilder.ForceRebuildLayoutImmediate(optionParent.GetComponent<RectTransform>());
        }

        public void Init(List<SubOptionData> _optionList, int layer, OptionSelectPopup p, int selected = -1)
        {
            Clear();

            parent = p;
            curLayer = layer;
            selectedOption = null;

            optionPrefab.SetActive(true);
            foreach (var data in _optionList)
            {
                var clone = Instantiate(optionPrefab, optionParent);
                var slot = clone.GetComponent<ChampionOptionSelectSlot>();
                slot.SetData(data, this);
                if (data.KEY == selected)
                {
                    selectedOption = slot;
                }
            }
            setColors(selectedOption);
            optionPrefab.SetActive(false);

            LayoutRebuilder.ForceRebuildLayoutImmediate(optionParent.GetComponent<RectTransform>());
        }

        public void Init(List<PartFusionData> _optionList, int layer, OptionSelectPopup p, int selected = -1)
        {
            Clear();

            parent = p;
            curLayer = layer;
            selectedOption = null;

            optionPrefab.SetActive(true);
            foreach (var data in _optionList)
            {
                var clone = Instantiate(optionPrefab, optionParent);
                var slot = clone.GetComponent<ChampionOptionSelectSlot>();
                slot.SetData(data, this);
                if (int.Parse(data.KEY) == selected)
                {
                    selectedOption = slot;
                }
            }
            setColors(selectedOption);
            optionPrefab.SetActive(false);

            LayoutRebuilder.ForceRebuildLayoutImmediate(optionParent.GetComponent<RectTransform>());
        }

        public void Init(List<SkillPassiveData> _optionList, int layer, OptionSelectPopup p, int selected = -1)
        {
            Clear();

            parent = p;
            curLayer = layer;
            selectedOption = null;

            optionPrefab.SetActive(true);
            foreach (var data in _optionList)
            {
                var clone = Instantiate(optionPrefab, optionParent);
                var slot = clone.GetComponent<ChampionOptionSelectSlot>();
                slot.SetData(data, this);
                if (data.KEY == selected)
                {
                    selectedOption = slot;
                }
            }
            setColors(selectedOption);
            optionPrefab.SetActive(false);

            LayoutRebuilder.ForceRebuildLayoutImmediate(optionParent.GetComponent<RectTransform>());
        }

        public void OnClickOption(ChampionOptionSelectSlot _option)
        {
            if (selectedOption != null && selectedOption == _option)
            {
                selectedOption = null;
            }

            selectedOption = _option;

            setColors(_option);

            parent.OnClickOption(curLayer, _option);
        }

        void setColors(ChampionOptionSelectSlot _selectedOption)
        {
            foreach(var option in optionParent.GetComponentsInChildren<ChampionOptionSelectSlot>())
            {
                if(selectedOption == option)
                    option.SetColor(selectedBackColor, Color.white);
                else
                    option.SetColor(Color.white, selectedBackColor);
            }
        }
    }
}