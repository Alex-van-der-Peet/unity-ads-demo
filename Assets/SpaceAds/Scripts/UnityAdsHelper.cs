﻿/// <summary>
/// UnityAdsHelper.cs - Written for Unity Ads Asset Store v1.1.2
///  by Nikkolai Davenport <nikkolai@unity3d.com> 
/// </summary>

using System;
using UnityEngine;
using System.Collections;
#if UNITY_IOS || UNITY_ANDROID
using UnityEngine.Advertisements;
#endif

public class UnityAdsHelper : MonoBehaviour 
{
	public string iosGameID = "24300";
	public string androidGameID = "24299";
	
	public bool enableTestMode = true;
	public bool showInfoLogs;
	public bool showDebugLogs;
	public bool showWarningLogs = true;
	public bool showErrorLogs = true;
	
	private static Action _handleFinished;
	private static Action _handleSkipped;
	private static Action _handleFailed;

#if UNITY_IOS || UNITY_ANDROID

	//--- Unity Ads Setup and Initialization

	void Start ()
	{
		Debug.Log("Running precheck for Unity Ads initialization...");

		string gameID = null;

	#if UNITY_IOS
		gameID = iosGameID;
	#elif UNITY_ANDROID
		gameID = androidGameID;
	#endif
		
		if (Advertisement.isInitialized)
		{
			Debug.LogWarning("Unity Ads is already initialized.");
		}
		else if (string.IsNullOrEmpty(gameID))
		{
			Debug.LogError("The game ID value is not set. A valid game ID is required to initialize Unity Ads.");
		}
		else
		{
			Advertisement.debugLevel = Advertisement.DebugLevel.NONE;	
			if (showInfoLogs) Advertisement.debugLevel    |= Advertisement.DebugLevel.INFO;
			if (showDebugLogs) Advertisement.debugLevel   |= Advertisement.DebugLevel.DEBUG;
			if (showWarningLogs) Advertisement.debugLevel |= Advertisement.DebugLevel.WARNING;
			if (showErrorLogs) Advertisement.debugLevel   |= Advertisement.DebugLevel.ERROR;
			
			if (enableTestMode && !Debug.isDebugBuild)
			{
				Debug.LogWarning("Development Build must be enabled in Build Settings to enable test mode for Unity Ads.");
			}
			
			bool isTestModeEnabled = Debug.isDebugBuild && enableTestMode;
			Debug.Log(string.Format("Precheck done. Initializing Unity Ads for game ID {0} with test mode {1}...",
			                        gameID, isTestModeEnabled ? "enabled" : "disabled"));

			Advertisement.Initialize(gameID,isTestModeEnabled);

			StartCoroutine(LogWhenUnityAdsIsInitialized());
		}
	}

	private IEnumerator LogWhenUnityAdsIsInitialized ()
	{
		float initStartTime = Time.time;

		do yield return new WaitForSeconds(0.1f);
		while (!initialized);

		Debug.Log(string.Format("Unity Ads was initialized in {0:F1} seconds.",Time.time - initStartTime));
		yield break;
	}
	
	//--- Static Helper Methods

	public static bool initialized { get { return IsInitialized(); }}
	
	public static bool IsInitialized () { return Advertisement.isInitialized; }
	
	public static bool IsReady (string zoneID = null) 
	{
		if (string.IsNullOrEmpty(zoneID)) zoneID = null;
		
		return Advertisement.isReady(zoneID);
	}

	public static void ShowAd (string zoneID = null, 
	                           Action handleFinished = null, 
	                           Action handleSkipped = null, 
	                           Action handleFailed = null)
	{
		if (string.IsNullOrEmpty(zoneID)) zoneID = null;

		_handleFinished = handleFinished;
		_handleSkipped = handleSkipped;
		_handleFailed = handleFailed;

		if (Advertisement.isReady(zoneID))
		{
			Debug.Log("Showing ad now...");
			
			ShowOptions options = new ShowOptions();
			options.resultCallback = HandleShowResult;
			options.pause = true;

			Advertisement.Show(zoneID,options);
		}
		else 
		{
			Debug.LogWarning(string.Format("Unable to show ad. The ad placement zone {0} is not ready.",
			                               object.ReferenceEquals(zoneID,null) ? "default" : zoneID));
		}
	}

	private static void HandleShowResult (ShowResult result)
	{
		switch (result)
		{
		case ShowResult.Finished:
			Debug.Log("The ad was successfully shown.");
			if (!object.ReferenceEquals(_handleFinished,null)) _handleFinished();
			break;
		case ShowResult.Skipped:
			Debug.LogWarning("The ad was skipped before reaching the end.");
			if (!object.ReferenceEquals(_handleSkipped,null)) _handleSkipped();
			break;
		case ShowResult.Failed:
			Debug.LogError("The ad failed to be shown.");
			if (!object.ReferenceEquals(_handleFailed,null)) _handleFailed();
			break;
		}
	}

#else

	void Start ()
	{
		Debug.LogWarning("Unity Ads is not supported under the current build platform.");
	}

	public static bool initialized { get { return IsInitialized(); }}
	
	public static bool IsInitialized () { return false; }
	
	public static bool IsReady (string zoneID = null) { return false; }

	public static void ShowAd (string zoneID = null)
	{
		Debug.LogError("Failed to show ad. Unity Ads is not supported under the current build platform.");
	}

#endif
}
