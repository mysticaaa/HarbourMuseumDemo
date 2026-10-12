using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HarbourMuseum.EditorTools
{
    [InitializeOnLoad]
    public static class MuseumSmoke
    {
        static double started;
        static bool testing;
        static int stage;
        static string reportPath;
        const string PendingKey="HarbourMuseum.SmokePending";
        static MuseumSmoke()
        {
            if(SessionState.GetBool(PendingKey,false))
            {
                reportPath=Path.Combine(Directory.GetParent(Application.dataPath).FullName,"MuseumPreview/InteractionValidation.txt");
                Attach();
            }
        }
        static void Attach()
        {
            testing=false;stage=0;started=EditorApplication.timeSinceStartup;
            EditorApplication.playModeStateChanged-=StateChanged;EditorApplication.update-=Tick;
            EditorApplication.playModeStateChanged+=StateChanged;EditorApplication.update+=Tick;
        }
        [MenuItem("Harbour Museum/Run interaction smoke test")]
        public static void Run()
        {
            reportPath=Path.Combine(Directory.GetParent(Application.dataPath).FullName,"MuseumPreview/InteractionValidation.txt");
            EditorSceneManager.OpenScene(MuseumSceneBuilder.ScenePath);
            SessionState.SetBool(PendingKey,true);Attach();
            EditorApplication.isPlaying=true;
        }
        static void StateChanged(PlayModeStateChange state)
        {
            if(state==PlayModeStateChange.EnteredPlayMode){testing=true;started=EditorApplication.timeSinceStartup;}
        }
        static void Tick()
        {
            if(!testing)
            {
                if(EditorApplication.isPlaying){testing=true;started=EditorApplication.timeSinceStartup;}
                else if(EditorApplication.timeSinceStartup-started>90)Fail(new Exception("Timed out entering Play Mode"));
                return;
            }
            if(EditorApplication.timeSinceStartup-started<2)return;
            try
            {
                if(stage==0)
                {
                    Tests();
                    var exp=UnityEngine.Object.FindObjectOfType<MuseumExperience>();exp.NavigateToStation(1);
                    ScreenCapture.CaptureScreenshot(Path.Combine(Directory.GetParent(Application.dataPath).FullName,"MuseumPreview/InteractivePreview.png"));
                    stage=1;started=EditorApplication.timeSinceStartup;
                }
                else
                {
                    EditorApplication.update-=Tick;EditorApplication.playModeStateChanged-=StateChanged;
                    SessionState.SetBool(PendingKey,false);
                    Debug.Log("HARBOUR_INTERACTION_SMOKE_SUCCESS");
                    if(Application.isBatchMode)EditorApplication.Exit(0);else EditorApplication.isPlaying=false;
                }
            }
            catch(Exception e){Fail(e);}
        }
        static void Tests()
        {
            var exp=UnityEngine.Object.FindObjectOfType<MuseumExperience>();Require(exp!=null,"Runtime controller exists");
            Require(!exp.IsInsideCabin && !exp.cabinRoot.activeSelf,"Starts in gallery");
            var shipRotation=exp.ship.localRotation;exp.NavigateToStation(1);exp.RotateShip(45);
            Require(Quaternion.Angle(shipRotation,exp.ship.localRotation)>40,"Ship rotates");
            exp.ResetShip();Require(Quaternion.Angle(shipRotation,exp.ship.localRotation)<.01f,"Ship resets to its saved orientation");
            exp.RotateShip(20);exp.EnterCabin();Require(exp.IsInsideCabin && exp.cabinRoot.activeSelf,"Cabin activates");
            Require(Vector3.Distance(exp.visitorCamera.transform.position,exp.cabinView.position)<.01f,"Camera enters cabin");
            foreach(var a in new[]{MuseumAction.InteriorSeating,MuseumAction.InteriorInstruments,MuseumAction.InteriorMachinery})exp.SelectAction(a);
            exp.ReturnToGallery();Require(!exp.IsInsideCabin && !exp.cabinRoot.activeSelf,"Returns to gallery");
            exp.NavigateToStation(2);
            foreach(var a in new[]{MuseumAction.TimelineWork,MuseumAction.TimelineMigration,MuseumAction.TimelineLife})exp.SelectAction(a);
            Require(exp.StoryPagesRead==3,"All three story pages are readable");
            exp.NavigateToStation(3);var helmetScale=exp.helmet.localScale;exp.RotateHelmet(30);exp.ZoomHelmet(.5f);
            Require(exp.helmet.localScale.x>helmetScale.x,"Helmet zoom enlarges original model");
            exp.ResetHelmet();Require((exp.helmet.localScale-helmetScale).sqrMagnitude<.0001f,"Helmet reset restores original scale");
            exp.ZoomHelmet(.5f);
            Require(exp.CompletedStations==3,"All three station journeys complete");
            var targets=UnityEngine.Object.FindObjectsOfType<MuseumTarget>(true);
            Physics.SyncTransforms();
            foreach(var target in targets)
            {
                if(target.action==MuseumAction.InteriorSeating || target.action==MuseumAction.InteriorInstruments || target.action==MuseumAction.InteriorMachinery)continue;
                Transform view=target.action==MuseumAction.Ship?exp.shipView:target.action==MuseumAction.Helmet?exp.helmetView:exp.timelineView;
                var collider=target.GetComponent<Collider>();Require(collider!=null,"Exhibit has ray collider: "+target.action);
                RaycastHit hit;Vector3 direction=collider.bounds.center-view.position;
                Require(Physics.Raycast(view.position,direction.normalized,out hit,direction.magnitude+1f) && hit.collider.GetComponentInParent<MuseumTarget>()==target,"Ray reaches exhibit: "+target.action);
            }
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath));
            File.WriteAllText(reportPath,"PASS - Unity Play Mode interaction validation\nGallery initial state\nShip rotation and original-orientation reset\nCabin activation, camera entry and return\nSeating, instruments and capstan points of interest\nThree story pages\nHelmet rotation, zoom and original-scale reset\nThree station completion\nPhysics rays hit five gallery exhibit targets\nQuest 3 and SteamVR hardware were not available for this validation.\n");
        }
        static void Require(bool value,string message){if(!value)throw new Exception(message);Debug.Log("MUSEUM_TEST_PASS: "+message);}
        static void Fail(Exception e)
        {
            EditorApplication.update-=Tick;EditorApplication.playModeStateChanged-=StateChanged;Debug.LogException(e);
            SessionState.SetBool(PendingKey,false);
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath));File.WriteAllText(reportPath,"FAIL\n"+e);
            if(Application.isBatchMode)EditorApplication.Exit(1);else EditorApplication.isPlaying=false;
        }
    }
}
