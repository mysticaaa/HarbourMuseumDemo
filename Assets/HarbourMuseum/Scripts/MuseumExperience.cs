using System;
using UnityEngine;

namespace HarbourMuseum
{
    /// <summary>Package-free desktop museum: exhibit handling, contextual stories and cabin exploration.</summary>
    public sealed class MuseumExperience : MonoBehaviour
    {
        public Camera visitorCamera;
        public Transform ship;
        public Transform helmet;
        public Transform galleryView;
        public Transform cabinView;
        public Transform overviewView;
        public Transform shipView;
        public Transform helmetView;
        public Transform timelineView;
        public GameObject cabinRoot;
        public GameObject shipCutawayRoot;
        public AudioClip narrationClip;
        public Vector2 galleryXBounds = new Vector2(-8.5f, 8.5f);
        public Vector2 galleryZBounds = new Vector2(-7.5f, 7.5f);
        public Vector2 cabinXBounds = new Vector2(-4.4f, 4.4f);
        public Vector2 cabinZBounds = new Vector2(36.6f, 43.4f);

        public bool IsInsideCabin { get { return inCabin; } }
        public int CompletedStations { get { return (ShipComplete ? 1 : 0) + (StoriesComplete ? 1 : 0) + (HelmetComplete ? 1 : 0); } }
        public int StoryPagesRead { get { return ((storyBits & 1) != 0 ? 1 : 0) + ((storyBits & 2) != 0 ? 1 : 0) + ((storyBits & 4) != 0 ? 1 : 0); } }
        public float HelmetZoom { get { return helmetScale; } }
        public MuseumAction SelectedAction { get { return selectedAction; } }
        private bool ShipComplete { get { return shipRotated && cabinEntered && cabinInspected; } }
        private bool StoriesComplete { get { return storyBits == 7; } }
        private bool HelmetComplete { get { return helmetRotated && helmetZoomed; } }

        private MuseumAction selectedAction = MuseumAction.Ship;
        private MuseumTarget hovered;
        private bool hasSelection, inCabin, showIntro = true, showHelp;
        private bool shipRotated, cabinEntered, cabinInspected, helmetRotated, helmetZoomed;
        private int storyBits, station;
        private float sessionSeconds, helmetScale = 1f, yaw, pitch;
        private float informationHeight = 1200f;
        private Quaternion originalShipRotation, originalHelmetRotation;
        private Vector3 originalHelmetScale = Vector3.one, initialCameraPosition;
        private Quaternion initialCameraRotation;
        private bool savedTransforms;
        private string selectedTitle, selectedDescription;
        private AudioSource narration;
        private Vector2 panelScroll;
        private Texture2D panelTexture, buttonTexture, activeTexture, quietTexture;
        private Font museumFont;
        private GUIStyle panelStyle, buttonStyle, activeButtonStyle, quietButtonStyle;
        private GUIStyle titleStyle, headingStyle, bodyStyle, smallStyle, eyebrowStyle, progressStyle;
        private readonly Color ink = new Color(.91f, .96f, .95f);
        private readonly Color muted = new Color(.68f, .8f, .8f);
        private readonly Color mint = new Color(.49f, .9f, .78f);
        private readonly Color gold = new Color(.97f, .76f, .4f);

        private float UIScale { get { return Mathf.Max(.25f, Mathf.Min(Screen.width / 1600f, Screen.height / 900f)); } }
        private float UIWidth { get { return Screen.width / UIScale; } }
        private float UIHeight { get { return Screen.height / UIScale; } }

        private void Awake()
        {
            SaveTransforms();
            if (cabinRoot != null) cabinRoot.SetActive(false);
            if (shipCutawayRoot != null) shipCutawayRoot.SetActive(false);
            if (narrationClip != null)
            {
                narration = gameObject.AddComponent<AudioSource>();
                narration.clip = narrationClip;
                narration.playOnAwake = false;
                narration.spatialBlend = 0f;
                narration.volume = .7f;
            }
            Application.targetFrameRate = 60;
        }

        private void Start()
        {
            SaveTransforms();
            SynchronizeLook();
            if (narration != null) narration.Play();
        }

        private void SaveTransforms()
        {
            if (savedTransforms) return;
            if (visitorCamera == null) visitorCamera = Camera.main;
            if (ship != null) originalShipRotation = ship.localRotation;
            if (helmet != null) { originalHelmetScale = helmet.localScale; originalHelmetRotation = helmet.localRotation; }
            if (visitorCamera != null)
            {
                initialCameraPosition = visitorCamera.transform.position;
                initialCameraRotation = visitorCamera.transform.rotation;
            }
            savedTransforms = true;
        }

        private void Update()
        {
            sessionSeconds += Time.deltaTime;
            if (visitorCamera == null) return;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (showHelp) showHelp = false;
                else if (showIntro) showIntro = false;
                else if (inCabin) ReturnToGallery();
                else ShowGallery();
            }
            if (Input.GetKeyDown(KeyCode.H)) showHelp = !showHelp;
            if (showHelp) { SetHover(null); return; }
            if (Input.GetKeyDown(KeyCode.Alpha1)) NavigateToStation(1);
            if (Input.GetKeyDown(KeyCode.Alpha2)) NavigateToStation(2);
            if (Input.GetKeyDown(KeyCode.Alpha3)) NavigateToStation(3);

            bool overUI = IsPointerOverUI();
            if (Input.GetMouseButton(1) && !overUI)
            {
                yaw += Input.GetAxisRaw("Mouse X") * 2.1f;
                pitch = Mathf.Clamp(pitch - Input.GetAxisRaw("Mouse Y") * 2.1f, -65f, 65f);
                visitorCamera.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
            }
            MoveVisitor();
            if (overUI) SetHover(null);
            else
            {
                Vector3 pointer = Input.GetMouseButton(1) ? new Vector3(Screen.width / 2f, Screen.height / 2f, 0f) : Input.mousePosition;
                RaycastHit hit;
                MuseumTarget target = Physics.Raycast(visitorCamera.ScreenPointToRay(pointer), out hit, 24f, ~0, QueryTriggerInteraction.Ignore)
                    ? hit.collider.GetComponentInParent<MuseumTarget>() : null;
                SetHover(target);
                if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E)) && hovered != null) SelectTarget(hovered);
            }
            if (hasSelection && !inCabin)
            {
                float turn = (Input.GetKey(KeyCode.RightArrow) ? 1f : 0f) - (Input.GetKey(KeyCode.LeftArrow) ? 1f : 0f);
                if (selectedAction == MuseumAction.Ship && turn != 0f) RotateShip(turn * 50f * Time.deltaTime);
                if (selectedAction == MuseumAction.Helmet)
                {
                    if (turn != 0f) RotateHelmet(turn * 50f * Time.deltaTime);
                    if (Input.GetKeyDown(KeyCode.PageUp)) ZoomHelmet(.15f);
                    if (Input.GetKeyDown(KeyCode.PageDown)) ZoomHelmet(-.15f);
                }
                if (Input.GetKeyDown(KeyCode.R))
                {
                    if (selectedAction == MuseumAction.Ship) ResetShip();
                    if (selectedAction == MuseumAction.Helmet) ResetHelmet();
                }
            }
        }

        private void MoveVisitor()
        {
            float horizontal = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
            float vertical = (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);
            if (horizontal == 0f && vertical == 0f) return;
            Transform view = visitorCamera.transform;
            Vector3 forward = Vector3.ProjectOnPlane(view.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(view.right, Vector3.up).normalized;
            Vector3 direction = (right * horizontal + forward * vertical).normalized;
            float distance = (Input.GetKey(KeyCode.LeftShift) ? 3.8f : 2.4f) * Time.deltaTime;
            RaycastHit obstacle;
            if (Physics.SphereCast(view.position, .24f, direction, out obstacle, distance + .035f, ~0, QueryTriggerInteraction.Ignore))
                distance = Mathf.Max(0f, obstacle.distance - .035f);
            Vector3 position = view.position + direction * distance;
            Vector2 boundsX = inCabin ? cabinXBounds : galleryXBounds;
            Vector2 boundsZ = inCabin ? cabinZBounds : galleryZBounds;
            position.x = Mathf.Clamp(position.x, boundsX.x, boundsX.y);
            position.z = Mathf.Clamp(position.z, boundsZ.x, boundsZ.y);
            view.position = position;
        }

        private void SetHover(MuseumTarget target)
        {
            if (hovered == target) return;
            if (hovered != null) hovered.SetHovered(false);
            hovered = target;
            if (hovered != null) hovered.SetHovered(true);
        }

        private void SelectTarget(MuseumTarget target)
        {
            SelectAction(target.action);
            selectedTitle = target.title;
            selectedDescription = target.description;
        }

        public void SelectAction(MuseumAction action)
        {
            showIntro = false;
            selectedAction = action;
            hasSelection = true;
            selectedTitle = selectedDescription = null;
            panelScroll = Vector2.zero;
            informationHeight = 1200f;
            switch (action)
            {
                case MuseumAction.Ship: station = 1; break;
                case MuseumAction.Helmet: station = 3; break;
                case MuseumAction.TimelineWork: station = 2; storyBits |= 1; break;
                case MuseumAction.TimelineMigration: station = 2; storyBits |= 2; break;
                case MuseumAction.TimelineLife: station = 2; storyBits |= 4; break;
                default: station = 1; if (inCabin) cabinInspected = true; break;
            }
        }

        public void NavigateToStation(int targetStation)
        {
            if (targetStation < 1 || targetStation > 3) return;
            if (inCabin) ReturnToGallery();
            showHelp = false;
            Transform viewpoint;
            if (targetStation == 1) { SelectAction(MuseumAction.Ship); viewpoint = shipView != null ? shipView : galleryView; }
            else if (targetStation == 2) { SelectAction(MuseumAction.TimelineWork); viewpoint = timelineView != null ? timelineView : galleryView; }
            else { SelectAction(MuseumAction.Helmet); viewpoint = helmetView != null ? helmetView : galleryView; }
            SetView(viewpoint);
        }

        public void RotateShip(float degrees)
        {
            if (ship == null || Mathf.Abs(degrees) < .0001f) return;
            ship.Rotate(Vector3.up, degrees, Space.World);
            shipRotated = true;
        }

        public void ResetShip()
        {
            if (ship != null) ship.localRotation = originalShipRotation;
            if (shipCutawayRoot != null) shipCutawayRoot.SetActive(false);
        }

        public void RotateHelmet(float degrees)
        {
            if (helmet == null || Mathf.Abs(degrees) < .0001f) return;
            helmet.Rotate(Vector3.up, degrees, Space.World);
            helmetRotated = true;
        }

        public void ZoomHelmet(float delta)
        {
            if (helmet == null) return;
            float next = Mathf.Clamp(helmetScale + delta, .65f, 1.65f);
            if (Mathf.Approximately(next, helmetScale)) return;
            helmetScale = next;
            helmet.localScale = originalHelmetScale * helmetScale;
            helmetZoomed = true;
        }

        public void ResetHelmet()
        {
            helmetScale = 1f;
            if (helmet == null) return;
            helmet.localScale = originalHelmetScale;
            helmet.localRotation = originalHelmetRotation;
        }

        public void EnterCabin()
        {
            if (cabinView == null || visitorCamera == null) return;
            if (cabinRoot != null) cabinRoot.SetActive(true);
            SetHover(null);
            inCabin = true;
            cabinEntered = true;
            SelectAction(MuseumAction.Ship);
            SetView(cabinView);
        }

        public void ReturnToGallery()
        {
            SetHover(null);
            inCabin = false;
            if (cabinRoot != null) cabinRoot.SetActive(false);
            SelectAction(MuseumAction.Ship);
            SetView(galleryView);
        }

        public void ShowGallery()
        {
            if (inCabin) ReturnToGallery();
            hasSelection = false;
            station = 0;
            showIntro = false;
            SetView(overviewView != null ? overviewView : galleryView);
        }

        private void SetView(Transform target)
        {
            if (visitorCamera == null) return;
            if (target != null) visitorCamera.transform.SetPositionAndRotation(target.position, target.rotation);
            else visitorCamera.transform.SetPositionAndRotation(initialCameraPosition, initialCameraRotation);
            SynchronizeLook();
        }

        private void SynchronizeLook()
        {
            if (visitorCamera == null) return;
            Vector3 angles = visitorCamera.transform.eulerAngles;
            yaw = angles.y;
            pitch = angles.x > 180f ? angles.x - 360f : angles.x;
        }

        private bool IsPointerOverUI()
        {
            if (showHelp) return true;
            Vector2 point = new Vector2(Input.mousePosition.x / UIScale, (Screen.height - Input.mousePosition.y) / UIScale);
            if (new Rect(20f, 20f, UIWidth - 40f, 94f).Contains(point)) return true;
            if (new Rect(20f, UIHeight - 102f, UIWidth - 40f, 82f).Contains(point)) return true;
            if (new Rect(32f, 138f, 285f, 164f).Contains(point)) return true;
            if (hasSelection && new Rect(UIWidth - 455f, 136f, 435f, UIHeight - 258f).Contains(point)) return true;
            return showIntro && IntroRect().Contains(point);
        }

        private Rect IntroRect() { return new Rect(32f, UIHeight - 378f, 584f, 256f); }

        private void OnGUI()
        {
            EnsureStyles();
            Matrix4x4 oldMatrix = GUI.matrix;
            Color oldColor = GUI.color;
            GUI.matrix = Matrix4x4.Scale(new Vector3(UIScale, UIScale, 1f));
            DrawHeader();
            DrawProgress();
            DrawFooter();
            if (hasSelection) DrawInformation();
            if (showIntro) DrawIntro();
            if (hovered != null && !showHelp)
            {
                string caption = string.IsNullOrEmpty(hovered.title) ? ActionTitle(hovered.action) : hovered.title;
                Rect hint = new Rect(UIWidth / 2f - 246f, UIHeight - 153f, 492f, 40f);
                GUI.Box(hint, GUIContent.none, quietButtonStyle);
                GUI.Label(hint, "CLICK / E  :  " + caption, progressStyle);
            }
            if (showHelp) DrawHelp();
            GUI.matrix = oldMatrix;
            GUI.color = oldColor;
        }

        private void DrawHeader()
        {
            GUI.Box(new Rect(20f, 20f, UIWidth - 40f, 94f), GUIContent.none, panelStyle);
            if (GUI.Button(new Rect(44f, 36f, 380f, 42f), "HARBOUR / MEMORY", titleStyle)) ShowGallery();
            GUI.Label(new Rect(46f, 79f, 405f, 24f), "AN INTERACTIVE MARITIME GALLERY", eyebrowStyle);
            int minutes = Mathf.FloorToInt(sessionSeconds / 60f), seconds = Mathf.FloorToInt(sessionSeconds % 60f);
            GUI.Label(new Rect(470f, 38f, 287f, 26f), string.Format("{0:00}:{1:00}  /  5 min visit", minutes, seconds), bodyStyle);
            GUI.Label(new Rect(470f, 73f, 270f, 24f), CompletedStations == 3 ? "3 / 3 stations complete." : "Explore at your own pace.", smallStyle);
            if (narration != null)
                DrawButton(new Rect(UIWidth - 830f, 41f, 154f, 51f), narration.isPlaying ? "PAUSE AUDIO" : "REPLAY AUDIO", ToggleNarration);
            float x = UIWidth - 660f;
            DrawButton(new Rect(x, 41f, 142f, 51f), "1  SHIP", delegate { NavigateToStation(1); }, station == 1);
            DrawButton(new Rect(x + 152f, 41f, 148f, 51f), "2  STORIES", delegate { NavigateToStation(2); }, station == 2);
            DrawButton(new Rect(x + 310f, 41f, 148f, 51f), "3  HELMET", delegate { NavigateToStation(3); }, station == 3);
            DrawButton(new Rect(x + 468f, 41f, 148f, 51f), "H  HELP", delegate { showHelp = true; });
        }

        private void DrawProgress()
        {
            GUI.Label(new Rect(34f, 138f, 280f, 28f), inCabin ? "INSIDE THE SHIP" : "YOUR FIVE-MINUTE VISIT", eyebrowStyle);
            DrawProgressRow(178f, "01", "Ship & cabin", ShipComplete);
            DrawProgressRow(216f, "02", "Lives by the sea", StoriesComplete);
            DrawProgressRow(254f, "03", "Diving helmet", HelmetComplete);
        }

        private void DrawProgressRow(float y, string number, string caption, bool complete)
        {
            GUI.color = complete ? mint : muted;
            GUI.Label(new Rect(34f, y, 38f, 27f), complete ? "+" : number, headingStyle);
            GUI.Label(new Rect(77f, y + 2f, 240f, 28f), caption, bodyStyle);
            GUI.color = Color.white;
        }

        private void DrawFooter()
        {
            GUI.Box(new Rect(20f, UIHeight - 102f, UIWidth - 40f, 82f), GUIContent.none, panelStyle);
            GUI.Label(new Rect(44f, UIHeight - 84f, 1120f, 29f), "W A S D  move   /   RIGHT MOUSE  look   /   CLICK or E  inspect   /   ESC  back", bodyStyle);
            GUI.Label(new Rect(46f, UIHeight - 51f, 1060f, 22f), "Original handmade assets  /  Interpretive reconstruction  /  Desktop prototype", smallStyle);
            if (inCabin) DrawButton(new Rect(UIWidth - 287f, UIHeight - 84f, 243f, 47f), "Return to gallery", ReturnToGallery, true);
            else DrawButton(new Rect(UIWidth - 252f, UIHeight - 84f, 208f, 47f), "Gallery view", ShowGallery);
        }

        private void DrawIntro()
        {
            Rect rect = IntroRect();
            GUI.Box(rect, GUIContent.none, panelStyle);
            GUI.Label(new Rect(rect.x + 24f, rect.y + 22f, rect.width - 48f, 29f), "WELCOME ABOARD", eyebrowStyle);
            GUI.Label(new Rect(rect.x + 24f, rect.y + 55f, rect.width - 48f, 46f), "Three objects. Many human stories.", headingStyle);
            GUI.Label(new Rect(rect.x + 24f, rect.y + 108f, rect.width - 48f, 76f), "Turn a ship, step into its working cabin, and examine a diver's helmet. Read three short stories about maritime life.", bodyStyle);
            DrawButton(new Rect(rect.x + 24f, rect.y + 195f, 272f, 43f), "Start with the ship", delegate { NavigateToStation(1); }, true);
            DrawButton(new Rect(rect.x + 308f, rect.y + 195f, 252f, 43f), "Explore freely", delegate { showIntro = false; });
        }

        private void DrawInformation()
        {
            Rect rect = new Rect(UIWidth - 455f, 136f, 435f, UIHeight - 258f);
            GUI.Box(rect, GUIContent.none, panelStyle);
            string label = inCabin ? "01 / SHIP INTERIOR" : (station == 1 ? "01 / THE SHIP" : station == 2 ? "02 / HUMAN STORIES" : "03 / WORKING DIVER");
            GUI.Label(new Rect(rect.x + 22f, rect.y + 20f, rect.width - 44f, 27f), label, eyebrowStyle);
            string title = !string.IsNullOrEmpty(selectedTitle) ? selectedTitle : ActionTitle(selectedAction);
            if (inCabin && selectedAction == MuseumAction.Ship) title = "A shared working space";
            Rect scrollRect = new Rect(rect.x + 20f, rect.y + 59f, rect.width - 34f, rect.height - 80f);
            float width = scrollRect.width - 24f;
            panelScroll = GUI.BeginScrollView(scrollRect, panelScroll, new Rect(0f, 0f, width, informationHeight), false, false);
            float y = 0f;
            AddText(ref y, width, title, headingStyle, 13f);
            if (inCabin)
            {
                string content = selectedAction == MuseumAction.Ship ? "Look around the cabin. Select the seating, instruments or machinery to discover how people shared a limited space at sea." : DefaultDescription(selectedAction);
                AddText(ref y, width, !string.IsNullOrEmpty(selectedDescription) ? selectedDescription : content, bodyStyle, 19f);
                AddText(ref y, width, "INSPECT A POINT OF INTEREST", eyebrowStyle, 12f);
                FlowButton(ref y, width, "Seating & everyday life", delegate { SelectAction(MuseumAction.InteriorSeating); }, selectedAction == MuseumAction.InteriorSeating);
                FlowButton(ref y, width, "Navigation instruments", delegate { SelectAction(MuseumAction.InteriorInstruments); }, selectedAction == MuseumAction.InteriorInstruments);
                FlowButton(ref y, width, "Capstan & shipboard work", delegate { SelectAction(MuseumAction.InteriorMachinery); }, selectedAction == MuseumAction.InteriorMachinery);
                AddText(ref y, width, "Evidence note: this cabin is an original interpretive reconstruction, not a measured interior from a named historical ship.", smallStyle, 15f);
            }
            else if (selectedAction == MuseumAction.Ship)
            {
                AddText(ref y, width, !string.IsNullOrEmpty(selectedDescription) ? selectedDescription : DefaultDescription(selectedAction), bodyStyle, 20f);
                AddText(ref y, width, "HANDLE THE MODEL", eyebrowStyle, 10f);
                PairButtons(ref y, width, "< Turn left", delegate { RotateShip(-30f); }, "Turn right >", delegate { RotateShip(30f); });
                FlowButton(ref y, width, "Reset model", ResetShip);
                if (shipCutawayRoot != null) FlowButton(ref y, width, shipCutawayRoot.activeSelf ? "Close cutaway" : "Show cutaway", delegate { shipCutawayRoot.SetActive(!shipCutawayRoot.activeSelf); });
                FlowButton(ref y, width, "Enter the cabin", EnterCabin, true);
                AddText(ref y, width, ShipComplete ? "STATION COMPLETE" : "Try: turn the ship, enter the cabin, inspect one detail.", smallStyle, 13f);
                AddText(ref y, width, "Evidence note: an original composite ship model, with simplified sailing-vessel features. It is not a replica of a named vessel.", smallStyle, 15f);
            }
            else if (selectedAction == MuseumAction.Helmet)
            {
                AddText(ref y, width, !string.IsNullOrEmpty(selectedDescription) ? selectedDescription : DefaultDescription(selectedAction), bodyStyle, 20f);
                AddText(ref y, width, "EXAMINE FROM EVERY ANGLE", eyebrowStyle, 10f);
                PairButtons(ref y, width, "< Turn left", delegate { RotateHelmet(-30f); }, "Turn right >", delegate { RotateHelmet(30f); });
                PairButtons(ref y, width, "- Zoom out", delegate { ZoomHelmet(-.15f); }, "+ Zoom in", delegate { ZoomHelmet(.15f); });
                FlowButton(ref y, width, "Reset helmet", ResetHelmet);
                AddText(ref y, width, string.Format("MODEL SIZE  {0:0}%", helmetScale * 100f), eyebrowStyle, 15f);
                AddText(ref y, width, HelmetComplete ? "STATION COMPLETE" : "Try: turn the helmet and change its size.", smallStyle, 15f);
                AddText(ref y, width, "Evidence note: the copper dome, front viewport and air fitting are a simplified reconstruction of surface-supplied diving equipment. Not a certified equipment model.", smallStyle, 15f);
                AddText(ref y, width, "Source context: Science Museum Group, diving helmet collection record. Material and air-hose context support this simplified exhibit.", smallStyle, 15f);
                FlowButton(ref y, width, "Open museum source", delegate { Application.OpenURL(SourceURL(MuseumAction.Helmet)); });
            }
            else
            {
                FlowButton(ref y, width, "01  Maritime work", delegate { SelectAction(MuseumAction.TimelineWork); }, selectedAction == MuseumAction.TimelineWork);
                FlowButton(ref y, width, "02  Migration journeys", delegate { SelectAction(MuseumAction.TimelineMigration); }, selectedAction == MuseumAction.TimelineMigration);
                FlowButton(ref y, width, "03  Everyday life", delegate { SelectAction(MuseumAction.TimelineLife); }, selectedAction == MuseumAction.TimelineLife);
                y += 7f;
                AddText(ref y, width, !string.IsNullOrEmpty(selectedDescription) ? selectedDescription : DefaultDescription(selectedAction), bodyStyle, 18f);
                AddText(ref y, width, "SAIL  >  STEAM  >  MODERN PORTS", eyebrowStyle, 12f);
                AddText(ref y, width, "The wall presents broad themes through changing maritime technologies, rather than a dated history of a specific port.", smallStyle, 14f);
                AddText(ref y, width, StorySource(selectedAction), smallStyle, 16f);
                FlowButton(ref y, width, "Open museum source", delegate { Application.OpenURL(SourceURL(selectedAction)); });
                AddText(ref y, width, StoriesComplete ? "ALL THREE STORIES READ" : string.Format("STORIES READ  {0} / 3", StoryPagesRead), eyebrowStyle, 15f);
            }
            GUI.EndScrollView();
            informationHeight = Mathf.Max(scrollRect.height, y + 18f);
        }

        private void ToggleNarration()
        {
            if (narration == null) return;
            if (narration.isPlaying) narration.Pause();
            else { narration.Stop(); narration.Play(); }
        }

        private void PairButtons(ref float y, float width, string left, Action leftAction, string right, Action rightAction)
        {
            DrawButton(new Rect(0f, y, (width - 10f) / 2f, 48f), left, leftAction);
            DrawButton(new Rect((width + 10f) / 2f, y, (width - 10f) / 2f, 48f), right, rightAction);
            y += 60f;
        }

        private void FlowButton(ref float y, float width, string caption, Action action, bool active = false)
        {
            DrawButton(new Rect(0f, y, width, 47f), caption, action, active);
            y += 59f;
        }

        private void AddText(ref float y, float width, string message, GUIStyle style, float gap)
        {
            float height = Mathf.Ceil(style.CalcHeight(new GUIContent(message), width));
            GUI.Label(new Rect(0f, y, width, height), message, style);
            y += height + gap;
        }

        private void DrawHelp()
        {
            GUI.color = new Color(0f, 0f, 0f, .68f);
            GUI.DrawTexture(new Rect(0f, 0f, UIWidth, UIHeight), Texture2D.whiteTexture);
            GUI.color = Color.white;
            Rect rect = new Rect(UIWidth / 2f - 340f, UIHeight / 2f - 281f, 680f, 562f);
            GUI.Box(rect, GUIContent.none, panelStyle);
            GUI.Label(new Rect(rect.x + 32f, rect.y + 27f, 610f, 35f), "Make the gallery your own", headingStyle);
            GUI.Label(new Rect(rect.x + 32f, rect.y + 77f, 610f, 50f), "Choose a station, then click an object or use the large controls on the right.", bodyStyle);
            string[] help = { "W A S D     Walk through the gallery or cabin", "RIGHT MOUSE     Hold and move to look around", "CLICK or E     Inspect a highlighted object", "1 / 2 / 3     Jump to ship, stories or helmet", "LEFT / RIGHT     Turn the selected model", "PAGE UP / DOWN     Change helmet size", "R     Reset the selected model", "ESC     Close help, leave the cabin or return home" };
            for (int i = 0; i < help.Length; i++) GUI.Label(new Rect(rect.x + 32f, rect.y + 149f + 37f * i, 612f, 32f), help[i], bodyStyle);
            DrawButton(new Rect(rect.x + 32f, rect.y + 477f, 616f, 52f), "Continue exploring", delegate { showHelp = false; }, true);
        }

        private void DrawButton(Rect rect, string caption, Action action, bool active = false)
        {
            if (GUI.Button(rect, caption, active ? activeButtonStyle : buttonStyle)) action();
        }

        private string ActionTitle(MuseumAction action)
        {
            switch (action)
            {
                case MuseumAction.Ship: return "A ship connects people";
                case MuseumAction.Helmet: return "Work beneath the surface";
                case MuseumAction.TimelineWork: return "Maritime work";
                case MuseumAction.TimelineMigration: return "Migration journeys";
                case MuseumAction.TimelineLife: return "Everyday life at sea";
                case MuseumAction.InteriorSeating: return "Living in a shared space";
                case MuseumAction.InteriorInstruments: return "Finding a way across water";
                default: return "Hauling lines together";
            }
        }

        private string DefaultDescription(MuseumAction action)
        {
            switch (action)
            {
                case MuseumAction.Ship: return "Follow the hull, deck and rigging to see how a vessel holds cargo and people together. Turn this handmade model, then step inside a reconstructed working cabin.";
                case MuseumAction.Helmet: return "A surface-supplied diver depended on an air hose, a helmet and a team above the water. Inspect the viewport, collar and air fitting: small details connect underwater work to people on the surface.";
                case MuseumAction.TimelineWork: return "Sailing, loading, repairing and navigating brought many kinds of labour together. Steam propulsion introduced new machinery and skills, while ports continued to rely on coordinated work aboard and ashore. This scene asks you to notice the people behind maritime technology.";
                case MuseumAction.TimelineMigration: return "Sea voyages carried people as well as goods. Passengers could face cramped accommodation, long journeys and uncertainty about arrival. Tickets, luggage and personal accounts can reveal individual experiences; this prototype represents those themes without inventing a passenger's biography.";
                case MuseumAction.TimelineLife: return "A ship was also a place to eat, rest, work and share limited space. Accommodation and routines varied by vessel, role and period. The cabin's seating, storage and work surfaces suggest everyday activities, while reminding us that objects alone cannot tell every person's story.";
                case MuseumAction.InteriorSeating: return "Benches and compact storage make the use of space visible. At sea, the same small room might support rest and conversation. These furnishings are illustrative, not evidence of a specific passenger's living conditions.";
                case MuseumAction.InteriorInstruments: return "A wheel, compass and chart desk connect observation with navigation. Different vessels and periods used different instruments. The arrangement here is a readable composite, designed to introduce the task of finding and maintaining a course.";
                default: return "A capstan is a rotating drum used to haul lines. Turning it required coordinated effort, connecting simple machinery to shipboard teamwork. This handmade capstan is an interpretive prop, not a working mechanism or an instruction for operating historical equipment.";
            }
        }

        private string StorySource(MuseumAction action)
        {
            string source = action == MuseumAction.TimelineWork
                ? "Source context: London Museum, Jobs that made London's docks run. Cargo handling and tallying reveal work behind a port."
                : action == MuseumAction.TimelineMigration
                    ? "Source context: Museums Victoria, Immigration to Victoria. Luggage, diaries and menus provide evidence of migration journeys."
                    : "Source context: Royal Museums Greenwich, Cutty Sark visitor guide. Cargo and crew spaces provide context for life aboard. ";
            return source + "\n\nEvidence note: broad interpretive themes, not a verified chronology for a named harbour. No invented quotations, people or exact dates are presented.";
        }

        private string SourceURL(MuseumAction action)
        {
            if (action == MuseumAction.Helmet) return "https://collection.sciencemuseumgroup.org.uk/objects/co8004872/diving-smoke-helmet";
            if (action == MuseumAction.TimelineWork) return "https://www.londonmuseum.org.uk/collections/london-stories/jobs-that-made-londons-docks-run/";
            if (action == MuseumAction.TimelineMigration) return "https://collections.museumsvictoria.com.au/articles/10647";
            return "https://www.rmg.co.uk/cutty-sark/top-things-to-do";
        }

        private void EnsureStyles()
        {
            if (panelStyle != null) return;
            museumFont = Font.CreateDynamicFontFromOSFont(new string[] { "Segoe UI", "Arial", "Liberation Sans" }, 24);
            panelTexture = RoundedTexture(new Color(.026f, .09f, .112f, .96f));
            buttonTexture = RoundedTexture(new Color(.1f, .22f, .25f));
            activeTexture = RoundedTexture(mint);
            quietTexture = RoundedTexture(new Color(.025f, .11f, .14f, .97f));
            panelStyle = new GUIStyle { border = new RectOffset(12, 12, 12, 12) };
            panelStyle.normal.background = panelTexture;
            buttonStyle = TextStyle(21, ink, false);
            buttonStyle.alignment = TextAnchor.MiddleCenter;
            buttonStyle.border = new RectOffset(12, 12, 12, 12);
            buttonStyle.normal.background = buttonTexture;
            buttonStyle.hover.background = quietTexture;
            buttonStyle.hover.textColor = gold;
            buttonStyle.active.background = activeTexture;
            buttonStyle.active.textColor = new Color(.03f, .13f, .13f);
            activeButtonStyle = new GUIStyle(buttonStyle);
            activeButtonStyle.normal.background = activeTexture;
            activeButtonStyle.normal.textColor = new Color(.025f, .12f, .12f);
            activeButtonStyle.fontStyle = FontStyle.Bold;
            quietButtonStyle = new GUIStyle(buttonStyle);
            quietButtonStyle.normal.background = quietTexture;
            titleStyle = TextStyle(31, ink, false);
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.hover.textColor = mint;
            headingStyle = TextStyle(27, ink, true);
            headingStyle.fontStyle = FontStyle.Bold;
            bodyStyle = TextStyle(21, ink, true);
            smallStyle = TextStyle(17, muted, true);
            eyebrowStyle = TextStyle(17, gold, true);
            eyebrowStyle.fontStyle = FontStyle.Bold;
            progressStyle = TextStyle(19, mint, false);
            progressStyle.alignment = TextAnchor.MiddleCenter;
        }

        private GUIStyle TextStyle(int size, Color color, bool wrap)
        {
            GUIStyle style = new GUIStyle { font = museumFont, fontSize = size, wordWrap = wrap, richText = false };
            style.normal.textColor = color;
            return style;
        }

        private Texture2D RoundedTexture(Color color)
        {
            const int size = 32;
            const float radius = 7f;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "Museum UI";
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(radius - x, x - (size - 1f - radius), 0f);
                float dy = Mathf.Max(radius - y, y - (size - 1f - radius), 0f);
                Color pixel = color;
                pixel.a *= Mathf.Clamp01(radius + .5f - Mathf.Sqrt(dx * dx + dy * dy));
                pixels[y * size + x] = pixel;
            }
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            return texture;
        }

        private void OnDisable() { SetHover(null); }
        private void OnDestroy()
        {
            if (panelTexture != null) Destroy(panelTexture);
            if (buttonTexture != null) Destroy(buttonTexture);
            if (activeTexture != null) Destroy(activeTexture);
            if (quietTexture != null) Destroy(quietTexture);
            if (museumFont != null) Destroy(museumFont);
        }
    }
}
