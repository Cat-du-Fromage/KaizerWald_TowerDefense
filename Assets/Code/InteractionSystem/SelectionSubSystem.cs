using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

using static TowerDefense.TowerDefenseUtils;
using static UnityEngine.Physics;
using static KWUtils.InputSystemExtension;
namespace TowerDefense
{
    public class SelectionSubSystem : MonoBehaviour
    {
        [SerializeField] private GameObject SelectionCanvas;
        private RectTransform SelectionBox;
        
        //Design HauptSystem of the SubSystem
        [SerializeField] private InteractionSystem HauptSystem;
        //Normally retrieve from HauptSystem
        [SerializeField] private Camera PlayerCamera;

        private Vector2 startLeftClick;
        private Vector2 endLeftClick => Mouse.current.position.ReadValue();
        
        private RaycastHit singleHit;
        private Ray singleRay;



        private void Awake()
        {
            HauptSystem ??= GetComponent<InteractionSystem>();
            SelectionBox = SelectionCanvas.transform.GetChild(0).GetComponent<RectTransform>();
        }

        private void Start()
        {
            PlayerCamera = HauptSystem.PlayerCamera;
        }

        public void OnStartLeftClick() => startLeftClick = GetMousePosition;

        public void OnLeftClickRelease()
        {
            singleRay = PlayerCamera.ScreenPointToRay(endLeftClick);
            SphereCast(singleRay,0.5f, out singleHit, Mathf.Infinity, UnitLayer);
            //select if not null, deselect otherwise
            HauptSystem.SelectionNotification(singleHit.transform);

            //RetrieveSelections(new Transform[2], ResizeSelectionBox());
            SelectionCanvas.SetActive(false);
        }

        public void OnMoveMouse()
        {
            SelectionCanvas.SetActive(true);
            ResizeSelectionBox();
        }

        private void RetrieveSelections(Transform[] AvailableUnits, Bounds bounds)
        {
            for (int i = 0; i < AvailableUnits.Length; i++)
            {
                /*
                if (IsEntityInSelectionBox(PlayerCamera.WorldToScreenPoint(AvailableUnits[i].position), bounds))
                {
                    if (!SelectionManager.Instance.IsSelected(SelectionManager.Instance.AvailableUnits[i]))
                    {
                        newlySelectedUnits.Add(SelectionManager.Instance.AvailableUnits[i]);
                    }
                    deselectedUnits.Remove(SelectionManager.Instance.AvailableUnits[i]);
                }
                else
                {
                    deselectedUnits.Add(SelectionManager.Instance.AvailableUnits[i]);
                    newlySelectedUnits.Remove(SelectionManager.Instance.AvailableUnits[i]);
                }
                */
            }
        }

        private bool IsEntityInSelectionBox(Vector2 position, Bounds bounds)
        {
            return position.x > bounds.min.x && position.x < bounds.max.x
                   && position.y > bounds.min.y && position.y < bounds.max.y;
        }
        
        //VISUAL UI FOR RECTANGLE
        //==============================================================================================================
        private Bounds ResizeSelectionBox()
        {
            float width = GetMousePosition.x - startLeftClick.x;
            float height = GetMousePosition.y - startLeftClick.y;

            SelectionBox.anchoredPosition = startLeftClick + new Vector2(width / 2, height / 2);
            SelectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));

            return new Bounds(SelectionBox.anchoredPosition, SelectionBox.sizeDelta);
        }
    }
}