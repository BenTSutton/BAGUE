using System;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    [Serializable]
    public class EnemyClips
    {
        public AudioClip die;
        public AudioClip hitSomething;
        public AudioClip move;
        public AudioClip[] spotPlayer;
        public AudioClip stunned;
        public AudioClip swing;
        public AudioClip[] takeDamage;
        public AudioClip windup;
    }

    [Serializable]
    public class PlayerClips
    {
        public AudioClip dash;
        public AudioClip enterParry;
        public AudioClip footstep;
        public AudioClip gunFire;
        public AudioClip gunReload;
        public AudioClip heavyAttack;
        public AudioClip land;
        public AudioClip[] lightAttacks;
        public AudioClip parrySuccess;
        public AudioClip cooldownFeedback;
        public AudioClip die;
        public AudioClip healed;
        public AudioClip takeDamage;
    }

    [Serializable]
    public class ShipClips
    {
        public AudioClip boarderAppears;
        public AudioClip desperation;
        public AudioClip dodgeShot;
        public AudioClip door;
        public AudioClip enemyShipExplosion;
        public AudioClip enemyShipHit;
        public AudioClip enemyShotWarning;
        public AudioClip stagedExplosion;
        public AudioClip openCannon;
        public AudioClip playerCannonFire;
        public AudioClip playerCannonReady;
        public AudioClip playerCannonReload;
        public AudioClip playerShieldHit;
        public AudioClip playerShipHit;
        public AudioClip roomTakeDamage;
        public AudioClip selectTarget;
        public AudioClip targetHover;
        public AudioClip useCloak;
    }

    [Serializable]
    public class UIClips
    {
        public AudioClip cancel;
        public AudioClip confirm;
        public AudioClip hover;
        public AudioClip negative;
        public AudioClip notice;
        public AudioClip positive;
        public AudioClip primaryClick;
        public AudioClip selectNode;
        public AudioClip showNodePanel;
        public AudioClip travel;
    }

    public static SFXManager Instance;

    [SerializeField] private AudioSource audioSource;

    [Header("Category Volumes")]
    [SerializeField, Range(0f, 1f)] private float enemyVolume = 0.9f;
    [SerializeField, Range(0f, 1f)] private float playerVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float shipVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float uiVolume = 0.65f;

    [Header("Positional Audio")]
    [SerializeField, Min(4)] private int worldSourcePoolSize = 20;
    [SerializeField, Min(0.1f)] private float worldMinDistance = 18f;
    [SerializeField, Min(1f)] private float worldMaxDistance = 180f;
    [SerializeField, Min(1f)] private float movementMaxDistance = 120f;

    [Header("Categorised SFX")]
    [SerializeField] private EnemyClips enemy = new EnemyClips();
    [SerializeField] private PlayerClips player = new PlayerClips();
    [SerializeField] private ShipClips ships = new ShipClips();
    [SerializeField] private UIClips ui = new UIClips();

    // Kept so existing scene and prefab button events continue to work.
    [HideInInspector] public AudioClip buttonClick;
    private readonly List<AudioSource> worldSources = new List<AudioSource>();
    private AudioListener cachedListener;
    private int lastSpecialUIFrame = -1;
    private float nextEnemyAlertSoundTime;
    private float nextEnemyDeathSoundTime;
    private float nextEnemyImpactSoundTime;
    private float nextEnemyMovementSoundTime;
    private float nextEnemyDamageSoundTime;
    private float nextEnemyWindupSoundTime;
    private float nextRoomDamageSoundTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        DontDestroyOnLoad(gameObject);
        BuildWorldSourcePool();
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || audioSource == null)
            return;

        audioSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
    }

    private static AudioClip GetRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
            return null;

        return clips[UnityEngine.Random.Range(0, clips.Length)];
    }

    private void BuildWorldSourcePool()
    {
        int sourceCount = Mathf.Max(4, worldSourcePoolSize);

        for (int i = 0; i < sourceCount; i++)
        {
            GameObject emitter = new GameObject($"World SFX {i + 1}");
            emitter.transform.SetParent(transform, false);

            AudioSource source = emitter.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.dopplerLevel = 0f;
            source.minDistance = worldMinDistance;
            source.maxDistance = worldMaxDistance;

            if (audioSource != null)
                source.outputAudioMixerGroup = audioSource.outputAudioMixerGroup;

            worldSources.Add(source);
        }
    }

    private bool TryPlayWorld(
        AudioClip clip,
        Vector3 position,
        float volumeScale,
        float maxDistance,
        float pitchVariation = 0f)
    {
        if (clip == null)
            return false;

        if (cachedListener == null || !cachedListener.isActiveAndEnabled)
            cachedListener = FindFirstObjectByType<AudioListener>();

        if (cachedListener == null)
        {
            PlaySFX(clip, volumeScale);
            return true;
        }

        Vector2 listenerPosition = cachedListener.transform.position;
        Vector2 soundPosition = position;

        if (Vector2.Distance(listenerPosition, soundPosition) > maxDistance)
            return false;

        AudioSource availableSource = null;

        for (int i = 0; i < worldSources.Count; i++)
        {
            if (!worldSources[i].isPlaying)
            {
                availableSource = worldSources[i];
                break;
            }
        }

        // Dropping excess low-priority sounds is cleaner than cutting off a sound
        // that is already playing.
        if (availableSource == null)
            return false;

        availableSource.transform.position = new Vector3(
            position.x,
            position.y,
            cachedListener.transform.position.z);
        availableSource.clip = clip;
        availableSource.volume = Mathf.Clamp01(volumeScale);
        availableSource.pitch = UnityEngine.Random.Range(
            1f - pitchVariation,
            1f + pitchVariation);
        availableSource.minDistance = worldMinDistance;
        availableSource.maxDistance = maxDistance;
        availableSource.Play();
        return true;
    }

    public void PlayWorldSFX(
        AudioClip clip,
        Vector3 position,
        float volumeScale = 1f)
    {
        TryPlayWorld(
            clip,
            position,
            volumeScale,
            worldMaxDistance);
    }

    public void PlayShipSFX(AudioClip clip, float relativeVolume = 1f)
    {
        PlaySFX(clip, shipVolume * relativeVolume);
    }

    public void PlayEnemyDie(Vector3 position)
    {
        if (Time.unscaledTime < nextEnemyDeathSoundTime)
            return;

        if (TryPlayWorld(enemy.die, position, enemyVolume, worldMaxDistance, 0.03f))
            nextEnemyDeathSoundTime = Time.unscaledTime + 0.08f;
    }

    public void PlayEnemyHitSomething(Vector3 position)
    {
        if (Time.unscaledTime < nextEnemyImpactSoundTime)
            return;

        if (TryPlayWorld(enemy.hitSomething, position, enemyVolume * 0.75f, worldMaxDistance, 0.03f))
            nextEnemyImpactSoundTime = Time.unscaledTime + 0.05f;
    }

    public void PlayEnemyMove(Vector3 position)
    {
        if (Time.unscaledTime < nextEnemyMovementSoundTime)
            return;

        if (TryPlayWorld(enemy.move, position, enemyVolume * 0.35f, movementMaxDistance, 0.05f))
            nextEnemyMovementSoundTime = Time.unscaledTime + 0.18f;
    }

    public void PlayEnemySpotPlayer(Vector3 position)
    {
        if (Time.unscaledTime < nextEnemyAlertSoundTime)
            return;

        if (TryPlayWorld(GetRandomClip(enemy.spotPlayer), position, enemyVolume * 0.95f, worldMaxDistance, 0.03f))
            nextEnemyAlertSoundTime = Time.unscaledTime + 0.15f;
    }

    public void PlayEnemyStunned(Vector3 position) =>
        TryPlayWorld(enemy.stunned, position, enemyVolume * 0.8f, worldMaxDistance, 0.03f);

    public void PlayEnemySwing(Vector3 position) =>
        TryPlayWorld(enemy.swing, position, enemyVolume * 0.7f, worldMaxDistance, 0.04f);

    public void PlayEnemyTakeDamage(Vector3 position)
    {
        if (Time.unscaledTime < nextEnemyDamageSoundTime)
            return;

        if (TryPlayWorld(GetRandomClip(enemy.takeDamage), position, enemyVolume * 0.75f, worldMaxDistance, 0.04f))
            nextEnemyDamageSoundTime = Time.unscaledTime + 0.04f;
    }

    public void PlayEnemyWindup(Vector3 position)
    {
        if (Time.unscaledTime < nextEnemyWindupSoundTime)
            return;

        if (TryPlayWorld(enemy.windup, position, enemyVolume * 0.6f, worldMaxDistance, 0.03f))
            nextEnemyWindupSoundTime = Time.unscaledTime + 0.08f;
    }

    public void PlayPlayerDash(Vector3 position) =>
        TryPlayWorld(player.dash, position, playerVolume * 0.7f, worldMaxDistance, 0.03f);

    public void PlayPlayerEnterParry(Vector3 position) =>
        TryPlayWorld(player.enterParry, position, playerVolume * 0.55f, worldMaxDistance, 0.02f);

    public void PlayPlayerFootstep(Vector3 position) =>
        TryPlayWorld(player.footstep, position, playerVolume * 0.6f, movementMaxDistance, 0.05f);

    public void PlayPlayerGunFire(Vector3 position) =>
        TryPlayWorld(player.gunFire, position, playerVolume * 0.9f, worldMaxDistance, 0.025f);

    public void PlayPlayerGunReload(Vector3 position) =>
        TryPlayWorld(player.gunReload, position, playerVolume * 0.7f, worldMaxDistance, 0.025f);

    public void PlayPlayerHeavyAttack(Vector3 position) =>
        TryPlayWorld(player.heavyAttack, position, playerVolume * 0.55f, worldMaxDistance, 0.025f);

    public void PlayPlayerLand(Vector3 position) =>
        TryPlayWorld(player.land, position, playerVolume * 0.4f, movementMaxDistance, 0.04f);

    public void PlayPlayerLightAttack(Vector3 position) =>
        TryPlayWorld(GetRandomClip(player.lightAttacks), position, playerVolume * 0.6f, worldMaxDistance, 0.035f);

    public void PlayPlayerParrySuccess(Vector3 position) =>
        TryPlayWorld(player.parrySuccess, position, playerVolume, worldMaxDistance);

    public void PlayPlayerCooldownFeedback(Vector3 position) =>
        TryPlayWorld(player.cooldownFeedback, position, playerVolume * 0.35f, worldMaxDistance);

    public void PlayPlayerDie(Vector3 position) =>
        TryPlayWorld(player.die, position, playerVolume, worldMaxDistance);

    public void PlayPlayerHealed(Vector3 position) =>
        TryPlayWorld(player.healed, position, playerVolume * 0.65f, worldMaxDistance, 0.02f);

    public void PlayPlayerTakeDamage(Vector3 position) =>
        TryPlayWorld(player.takeDamage, position, playerVolume * 0.85f, worldMaxDistance, 0.02f);

    public void PlayBoarderAppears(Vector3 position) =>
        TryPlayWorld(ships.boarderAppears, position, shipVolume * 0.6f, worldMaxDistance);

    public void PlayDoor(Vector3 position) =>
        TryPlayWorld(ships.door, position, shipVolume * 0.45f, movementMaxDistance, 0.025f);

    public void PlayRoomTakeDamage(Vector3 position)
    {
        if (Time.unscaledTime < nextRoomDamageSoundTime)
            return;

        if (TryPlayWorld(ships.roomTakeDamage, position, shipVolume * 0.5f, worldMaxDistance, 0.025f))
            nextRoomDamageSoundTime = Time.unscaledTime + 0.08f;
    }

    public void PlayDesperation() => PlayShipSFX(ships.desperation, 0.8f);
    public void PlayDodgeShot() => PlayShipSFX(ships.dodgeShot, 0.7f);
    public void PlayEnemyShipExplosion() => PlayShipSFX(ships.enemyShipExplosion);
    public void PlayEnemyShipHit() => PlayShipSFX(ships.enemyShipHit, 0.6f);
    public void PlayEnemyShotWarning() => PlayShipSFX(ships.enemyShotWarning, 0.8f);
    public void PlayStagedExplosion() => PlayShipSFX(ships.stagedExplosion, 0.75f);
    public void PlayOpenCannon() => PlayShipSFX(ships.openCannon, 0.55f);
    public void PlayPlayerCannonFire() => PlayShipSFX(ships.playerCannonFire, 0.85f);
    public void PlayPlayerCannonReady() => PlayShipSFX(ships.playerCannonReady, 0.6f);
    public void PlayPlayerCannonReload() => PlayShipSFX(ships.playerCannonReload, 0.4f);
    public void PlayPlayerShieldHit() => PlayShipSFX(ships.playerShieldHit, 0.75f);
    public void PlayPlayerShipHit() => PlayShipSFX(ships.playerShipHit, 0.75f);
    public void PlaySelectTarget() => PlayShipSFX(ships.selectTarget, 0.55f);
    public void PlayTargetHover() => PlayShipSFX(ships.targetHover, 0.3f);
    public void PlayUseCloak() => PlayShipSFX(ships.useCloak, 0.7f);

    private void PlaySpecialUI(AudioClip clip)
    {
        lastSpecialUIFrame = Time.frameCount;
        PlaySFX(clip, uiVolume);
    }

    public void PlayCancel() => PlaySpecialUI(ui.cancel);
    public void PlayConfirm() => PlaySpecialUI(ui.confirm);
    public void PlayHover() => PlaySpecialUI(ui.hover);
    public void PlayNegative() => PlaySpecialUI(ui.negative);
    public void PlayNotice() => PlaySpecialUI(ui.notice);
    public void PlayPositive() => PlaySpecialUI(ui.positive);

    public void PlayButtonClick()
    {
        if (lastSpecialUIFrame == Time.frameCount)
            return;

        PlaySFX(ui.primaryClick != null ? ui.primaryClick : buttonClick, uiVolume);
    }

    public void PlaySelectNode() => PlaySpecialUI(ui.selectNode);
    public void PlayShowNodePanel() => PlaySpecialUI(ui.showNodePanel);
    public void PlayTravel() => PlaySpecialUI(ui.travel);

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        AutoAssignMissingClips();
    }

    [ContextMenu("Auto Assign Missing SFX Clips")]
    private void AutoAssignMissingClips()
    {
        if (enemy == null)
            enemy = new EnemyClips();
        if (player == null)
            player = new PlayerClips();
        if (ships == null)
            ships = new ShipClips();
        if (ui == null)
            ui = new UIClips();

        Assign(ref enemy.die, "Enemy/EnemyDie.wav");
        Assign(ref enemy.hitSomething, "Enemy/EnemyHitSomething.wav");
        Assign(ref enemy.move, "Enemy/EnemyMove.wav");
        AssignArray(ref enemy.spotPlayer, "Enemy/EnemySpotPlayer.wav", "Enemy/EnemySpotPlayer2.wav");
        Assign(ref enemy.stunned, "Enemy/EnemyStunned.wav");
        Assign(ref enemy.swing, "Enemy/EnemySwing.wav");
        AssignArray(ref enemy.takeDamage, "Enemy/EnemyTakeDamage (1).wav", "Enemy/EnemyTakeDamage (2).wav");
        Assign(ref enemy.windup, "Enemy/EnemyWindup.wav");

        Assign(ref player.dash, "Player/Dash.wav");
        Assign(ref player.enterParry, "Player/EnterParry.wav");
        Assign(ref player.footstep, "Player/Footstep.wav");
        Assign(ref player.gunFire, "Player/GunFire.mp3");
        Assign(ref player.gunReload, "Player/GunReload.mp3");
        Assign(ref player.heavyAttack, "Player/HeavyAttack.wav");
        Assign(ref player.land, "Player/Land.wav");
        AssignArray(ref player.lightAttacks, "Player/LightAttack.wav", "Player/LightAttack (1).wav", "Player/LightAttack (2).wav", "Player/LightAttack (3).wav");
        Assign(ref player.parrySuccess, "Player/ParrySuccess.wav");
        Assign(ref player.cooldownFeedback, "Player/PlayerCooldownFeedback.wav");
        Assign(ref player.die, "Player/PlayerDie.wav");
        Assign(ref player.healed, "Player/PlayerHealed.wav");
        Assign(ref player.takeDamage, "Player/PlayerTakeDamage.wav");

        Assign(ref ships.boarderAppears, "Ships/BoarderAppears.wav");
        Assign(ref ships.desperation, "Ships/Desperation.wav");
        Assign(ref ships.dodgeShot, "Ships/DodgeShot.wav");
        Assign(ref ships.door, "Ships/Door.wav");
        Assign(ref ships.enemyShipExplosion, "Ships/EnemyShipExplosion.wav");
        Assign(ref ships.enemyShipHit, "Ships/EnemyShipHit.wav");
        Assign(ref ships.enemyShotWarning, "Ships/EnemyShotWarning.wav");
        Assign(ref ships.stagedExplosion, "Ships/Explosion 2.wav");
        Assign(ref ships.openCannon, "Ships/OpenCannon.wav");
        Assign(ref ships.playerCannonFire, "Ships/PlayerCannonFire.wav");
        Assign(ref ships.playerCannonReady, "Ships/PlayerCannonReady.wav");
        Assign(ref ships.playerCannonReload, "Ships/PlayerCannonReload.wav");
        Assign(ref ships.playerShieldHit, "Ships/PlayerShieldHit.wav");
        Assign(ref ships.playerShipHit, "Ships/PlayerShipHit.wav");
        Assign(ref ships.roomTakeDamage, "Ships/RoomTakeDamage.wav");
        Assign(ref ships.selectTarget, "Ships/SelectTarget.wav");
        Assign(ref ships.targetHover, "Ships/TargetHover.wav");
        Assign(ref ships.useCloak, "Ships/UseCloak.wav");

        Assign(ref ui.cancel, "UI/Cancel.wav");
        Assign(ref ui.confirm, "UI/Confirm.wav");
        Assign(ref ui.hover, "UI/Hover.wav");
        Assign(ref ui.negative, "UI/Negative.wav");
        Assign(ref ui.notice, "UI/Notice.wav");
        Assign(ref ui.positive, "UI/Positive.wav");
        Assign(ref ui.primaryClick, "UI/PrimaryClick.wav");
        Assign(ref ui.selectNode, "UI/SelectNodeToGoInto.wav");
        Assign(ref ui.showNodePanel, "UI/ShowNodePanel.wav");
        Assign(ref ui.travel, "UI/Travel.wav");
    }

    private static void Assign(ref AudioClip clip, string relativePath)
    {
        if (clip == null)
        {
            clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(
                "Assets/Sound/SFX/" + relativePath);
        }
    }

    private static void AssignArray(ref AudioClip[] clips, params string[] relativePaths)
    {
        if (clips == null || clips.Length == 0)
            clips = new AudioClip[relativePaths.Length];

        int clipCount = Mathf.Min(clips.Length, relativePaths.Length);

        for (int i = 0; i < clipCount; i++)
        {
            if (clips[i] == null)
            {
                clips[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(
                    "Assets/Sound/SFX/" + relativePaths[i]);
            }
        }
    }
#endif
}
