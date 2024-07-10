using MDPro3;
using MDPro3.YGOSharp.OCGWrapper.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TextureContainer : ScriptableObject
{
    [Header("Common")]
    public Sprite black;
    public Sprite transparent;
    public Sprite unknownCard;
    public Sprite unknownArt;
    public Sprite cardBackDefault;
    [Header("Card Frame")]
    public Sprite cardFrameNormal;
    public Sprite cardFrameEffect;
    public Sprite cardFrameRitual;
    public Sprite cardFrameFusion;
    public Sprite cardFrameObelisk;
    public Sprite cardFrameOsiris;
    public Sprite cardFrameRa;
    public Sprite cardFrameSpell;
    public Sprite cardFrameTrap;
    public Sprite cardFrameToken;
    public Sprite cardFrameSynchro;
    public Sprite cardFrameDarkSynchro;
    public Sprite cardFrameXyz;
    public Sprite cardFramePendulumNormal;
    public Sprite cardFramePendulumEffect;
    public Sprite cardFramePendulumXyz;
    public Sprite cardFramePendulumSynchro;
    public Sprite cardFramePendulumFusion;
    public Sprite cardFrameLink;
    public Sprite cardFramePendulumRitual;
    public Sprite cardFrameNormalOF;
    public Sprite cardFrameEffectOF;
    public Sprite cardFrameRitualOF;
    public Sprite cardFrameFusionOF;
    public Sprite cardFrameObeliskOF;
    public Sprite cardFrameOsirisOF;
    public Sprite cardFrameRaOF;
    public Sprite cardFrameSpellOF;
    public Sprite cardFrameTrapOF;
    public Sprite cardFrameTokenOF;
    public Sprite cardFrameSynchroOF;
    public Sprite cardFrameDarkSynchroOF;
    public Sprite cardFrameXyzOF;
    public Sprite cardFramePendulumNormalOF;
    public Sprite cardFramePendulumEffectOF;
    public Sprite cardFramePendulumXyzOF;
    public Sprite cardFramePendulumSynchroOF;
    public Sprite cardFramePendulumFusionOF;
    public Sprite cardFrameLinkOF;
    public Sprite cardFramePendulumRitualOF;
    [Header("Card Frame Mask")]
    public Texture2D cardFrameMask;
    public Texture2D cardFrameMaskLink;
    public Texture2D cardFrameMaskPendulum;
    public Texture2D cardKiraMask;
    public Texture2D cardKiraMaskLink;
    public Texture2D cardKiraMaskPendulum;
    public Texture2D cardNormal;
    public Texture2D cardNormalLink;
    public Texture2D cardNormalPendulum;
    [Header("Card Attribute")]
    public Sprite attributeLight;
    public Sprite attributeDark;
    public Sprite attributeWater;
    public Sprite attributeFire;
    public Sprite attributeEarth;
    public Sprite attributeWind;
    public Sprite attributeDivine;
    public Sprite attributeSpell;
    public Sprite attributeTrap;
    [Header("CardType")]
    public Sprite typeNone;
    public Sprite typeCounter;
    public Sprite typeField;
    public Sprite typeEquip;
    public Sprite typeContinuous;
    public Sprite typeQuickPlay;
    public Sprite typeRitual;
    public Sprite typeLevel;
    public Sprite typeRank;
    public Sprite typePendulum;
    public Sprite typeLink;
    public Sprite typeLevelOff;
    public Sprite typeLinkOff;
    public Sprite typeLevelNone;
    public Sprite typeLevelRank;
    [Header("CardLimit")]
    public Sprite banned;
    public Sprite limit1;
    public Sprite limit2;
    [Header("CardRace")]
    public Sprite raceDragon;
    public Sprite raceZombie;
    public Sprite raceFiend;
    public Sprite racePyro;
    public Sprite raceSeaSerpent;
    public Sprite raceRock;
    public Sprite raceMachine;
    public Sprite raceFish;
    public Sprite raceDinosaur;
    public Sprite raceInsect;
    public Sprite raceBeast;
    public Sprite raceBeastWarrior;
    public Sprite racePlant;
    public Sprite raceAqua;
    public Sprite raceWarrior;
    public Sprite raceWindBeast;
    public Sprite raceFairy;
    public Sprite raceSpellCaster;
    public Sprite raceThunder;
    public Sprite raceReptile;
    public Sprite racePsycho;
    public Sprite raceWyrm;
    public Sprite raceCyberse;
    public Sprite raceDivineBeast;
    public Sprite raceIllustion;
    public Sprite raceCreatorGod;

    [Header("CardCounter")]
    public Sprite counterAlien;
    public Sprite counterAthlete;
    public Sprite counterBalloon;
    public Sprite counterBarrel;
    public Sprite counterBF;
    public Sprite counterBurn;
    public Sprite counterBushido;
    public Sprite counterChaos;
    public Sprite counterChronicle;
    public Sprite counterClock;
    public Sprite counterD;
    public Sprite counterDeath;
    public Sprite counterDefect;
    public Sprite counterDeformer;
    public Sprite counterDestiny;
    public Sprite counterDonguri;
    public Sprite counterDouble;
    public Sprite counterDragonic;
    public Sprite counterEarthBind;
    public Sprite counterEM;
    public Sprite counterFireStar;
    public Sprite counterFlower;
    public Sprite counterFog;
    public Sprite counterGardna;
    public Sprite counterGate;
    public Sprite counterGem;
    public Sprite counterGenex;
    public Sprite counterGG;
    public Sprite counterGirl;
    public Sprite counterGreed;
    public Sprite counterGuard;
    public Sprite counterGuard2;
    public Sprite counterHopeSlash;
    public Sprite counterHoukai;
    public Sprite counterHyper;
    public Sprite counterIce;
    public Sprite counterIllusion;
    public Sprite counterJunk;
    public Sprite counterKaiju;
    public Sprite counterKarakuri;
    public Sprite counterKattobing;
    public Sprite counterKyoumei;
    public Sprite counterMagic;
    public Sprite counterNormal;
    public Sprite counterOcean;
    public Sprite counterOrbital;
    public Sprite counterOtoshidama;
    public Sprite counterOunokagi;
    public Sprite counterPhantasm;
    public Sprite counterPiece;
    public Sprite counterPlant;
    public Sprite counterPolice;
    public Sprite counterPredator;
    public Sprite counterPsycho;
    public Sprite counterPumpkin;
    public Sprite counterRabbit;
    public Sprite counterScales;
    public Sprite counterShark;
    public Sprite counterShine;
    public Sprite counterSignal;
    public Sprite counterSound;
    public Sprite counterStone;
    public Sprite counterString;
    public Sprite counterSummon;
    public Sprite counterThunder;
    public Sprite counterVenemy;
    public Sprite counterVenom;
    public Sprite counterWedge;
    public Sprite counterWorm;
    public Sprite counterYosen;
    public Sprite counterZushin;

    [Header("Button Icon")]
    public Sprite[] battle;
    public Sprite[] select;
    public Sprite[] spSummon;
    public Sprite[] activate;
    public Sprite[] summon;
    public Sprite[] setSpell;
    public Sprite[] setMonster;
    public Sprite[] toAttack;
    public Sprite[] toDefense;
    public Sprite[] setPendulum;
    public Sprite[] penSummon;
    public Sprite[] cancel;
    public Sprite[] decide;
    public Sprite[] onTiming;
    public Sprite[] offTiming;
    public Sprite[] autoTiming;
    public Sprite[] onLog;
    public Sprite[] offLog;

    [Header("Location Icon")]
    public Sprite locationDeck;
    public Sprite locationExtra;
    public Sprite locationHand;
    public Sprite locationGrave;
    public Sprite locationRemoved;
    public Sprite locationFieldMagic;
    public Sprite locationOverlay;
    public Sprite locationSearch;
    public Sprite locationMyField;
    public Sprite locationMyMZone0;
    public Sprite locationMyMZone1;
    public Sprite locationMyMZone2;
    public Sprite locationMyMZone3;
    public Sprite locationMyMZone4;
    public Sprite locationMyMZone5;
    public Sprite locationMyMZone6;
    public Sprite locationMySZone0;
    public Sprite locationMySZone1;
    public Sprite locationMySZone2;
    public Sprite locationMySZone3;
    public Sprite locationMySZone4;
    public Sprite locationOpField;
    public Sprite locationOpMZone0;
    public Sprite locationOpMZone1;
    public Sprite locationOpMZone2;
    public Sprite locationOpMZone3;
    public Sprite locationOpMZone4;
    public Sprite locationOpMZone5;
    public Sprite locationOpMZone6;
    public Sprite locationOpSZone0;
    public Sprite locationOpSZone1;
    public Sprite locationOpSZone2;
    public Sprite locationOpSZone3;
    public Sprite locationOpSZone4;
    [Header("Card Controller Icon")]
    public Sprite controllerMe;
    public Sprite controllerOp;
    public Sprite controllerOther;
    public Sprite controllerOther2;
    [Header("Card List Location Icon")]
    public Sprite listMyDeck;
    public Sprite listOpDeck;
    public Sprite listMyExtra;
    public Sprite listOpExtra;
    public Sprite listMyGrave;
    public Sprite listOpGrave;
    public Sprite listMyRemoved;
    public Sprite listOpRemoved;
    public Sprite listMyXyz;
    public Sprite listOpXyz;
    [Header("Card Affect")]
    public Sprite CardAffectDisable;
    public Sprite CardAffectEquip;
    public Sprite CardAffectField;
    public Sprite CardAffectPermanent;
    public Sprite CardAffectPower;
    public Sprite CardAffectTarget;
    [Header("Link Count")]
    public Sprite link1;
    public Sprite link2;
    public Sprite link3;
    public Sprite link4;
    public Sprite link5;
    public Sprite link6;
    public Sprite link1R;
    public Sprite link2R;
    public Sprite link3R;
    public Sprite link4R;
    public Sprite link5R;
    public Sprite link6R;
    public Sprite link7R;
    public Sprite link8R;
    [Header("Chain Circle Num")]
    public Sprite chainCircleNum0;
    public Sprite chainCircleNum1;
    public Sprite chainCircleNum2;
    public Sprite chainCircleNum3;
    public Sprite chainCircleNum4;
    public Sprite chainCircleNum5;
    public Sprite chainCircleNum6;
    public Sprite chainCircleNum7;
    public Sprite chainCircleNum8;
    public Sprite chainCircleNum9;
    [Header("Chain Num Set")]
    public Sprite chainNumSet0;
    public Sprite chainNumSet1;
    public Sprite chainNumSet2;
    public Sprite chainNumSet3;
    public Sprite chainNumSet4;
    public Sprite chainNumSet5;
    public Sprite chainNumSet6;
    public Sprite chainNumSet7;
    public Sprite chainNumSet8;
    public Sprite chainNumSet9;
    [Header("Window")]
    public Sprite toggleM;
    public Sprite toggleM_On;
    public Sprite toggleM_Over;

    [Header("Rank")]
    public Sprite rankBG01;
    public Sprite rankBG02;
    public Sprite rankBG03;
    public Sprite rankBG04;
    public Sprite rankBG05;
    public Sprite rankBG06;
    public Sprite rankBG07;
    public Sprite rankBG08;
    public Sprite rankIcon01;
    public Sprite rankIcon02;
    public Sprite rankIcon03;
    public Sprite rankIcon04;
    public Sprite rankIcon05;
    public Sprite rankIcon06;
    public Sprite rankIcon07;
    public Sprite rankIcon08;
    public Sprite rankTier01;
    public Sprite rankTier02;
    public Sprite rankTier03;
    public Sprite rankTier04;
    public Sprite rankTier05;

    [Header("Other")]
    public Texture2D fxt_Arrow;
    public Texture2D fxt_Arrow_002;
    public Texture2D fxt_Arrow_003;
    public Texture2D fxt_Arrow_004;
    public Texture2D fxt_msk_005;

    public List<Sprite> GetLocationIcons(GPS p)
    {
        var returnValue = new List<Sprite>();
        if((p.location & (uint)CardLocation.Onfield) > 0 
            && (p.location & (uint)CardLocation.Overlay) == 0)
        {
            if ((p.location & (uint)CardLocation.SpellZone) > 0 && p.sequence == 5)
            {
                returnValue.Add(locationFieldMagic);
                returnValue.Add(p.controller == 0 ? controllerMe : controllerOp);
                return returnValue;
            }
            if((p.location & (uint)CardLocation.MonsterZone) > 0)
            {
                switch (p.sequence)
                {
                    case 0:
                        returnValue.Add(p.controller == 0 ? locationMyMZone0 : locationOpMZone0);
                        break;
                    case 1:
                        returnValue.Add(p.controller == 0 ? locationMyMZone1 : locationOpMZone1);
                        break;
                    case 2:
                        returnValue.Add(p.controller == 0 ? locationMyMZone2 : locationOpMZone2);
                        break;
                    case 3:
                        returnValue.Add(p.controller == 0 ? locationMyMZone3 : locationOpMZone3);
                        break;
                    case 4:
                        returnValue.Add(p.controller == 0 ? locationMyMZone4 : locationOpMZone4);
                        break;
                    case 5:
                        returnValue.Add(p.controller == 0 ? locationMyMZone5 : locationOpMZone5);
                        break;
                    case 6:
                        returnValue.Add(p.controller == 0 ? locationMyMZone6 : locationOpMZone6);
                        break;
                }
            }
            else
            {
                switch (p.sequence)
                {
                    case 0:
                        returnValue.Add(p.controller == 0 ? locationMySZone0 : locationOpSZone0);
                        break;
                    case 1:
                        returnValue.Add(p.controller == 0 ? locationMySZone1 : locationOpSZone1);
                        break;
                    case 2:
                        returnValue.Add(p.controller == 0 ? locationMySZone2 : locationOpSZone2);
                        break;
                    case 3:
                        returnValue.Add(p.controller == 0 ? locationMySZone3 : locationOpSZone3);
                        break;
                    case 4:
                        returnValue.Add(p.controller == 0 ? locationMySZone4 : locationOpSZone4);
                        break;
                }
            }
        }
        else
        {
            if ((p.location & (uint)CardLocation.Overlay) > 0)
                returnValue.Add(locationOverlay);
            else if ((p.location & (uint)CardLocation.Deck) > 0)
                returnValue.Add(locationDeck);
            else if ((p.location & (uint)CardLocation.Extra) > 0)
                returnValue.Add(locationExtra);
            else if ((p.location & (uint)CardLocation.Hand) > 0)
                returnValue.Add(locationHand);
            else if ((p.location & (uint)CardLocation.Grave) > 0)
                returnValue.Add(locationGrave);
            else if ((p.location & (uint)CardLocation.Removed) > 0)
                returnValue.Add(locationRemoved);

            returnValue.Add(p.controller == 0 ? controllerMe : controllerOp);
        }
        return returnValue;
    }

    int[] rankRange = new int[]
    {
        1000,
        1100,
        1200,
        1300,
        1400,
        1500,
        1600,
        1700
    };

    public List<Sprite> GetRankSprites(int rank)
    {
        var returnValue = new List<Sprite>();
        if(rank < rankRange[1])
        {
            returnValue.Add(rankBG01);
            returnValue.Add(rankIcon01);
            returnValue.Add(GetRankTier(rankRange[0], rankRange[1], rank));
            returnValue.Add(transparent);
            returnValue.Add(transparent);
        }
        else if(rank < rankRange[2])
        {
            returnValue.Add(rankBG02);
            returnValue.Add(rankIcon02);
            returnValue.Add(GetRankTier(rankRange[1], rankRange[2], rank));
            returnValue.Add(transparent);
            returnValue.Add(transparent);
        }
        else if(rank < rankRange[3])
        {
            returnValue.Add(rankBG03);
            returnValue.Add(rankIcon03);
            returnValue.Add(GetRankTier(rankRange[2], rankRange[3], rank));
            returnValue.Add(transparent);
            returnValue.Add(transparent);
        }
        else if (rank < rankRange[4])
        {
            returnValue.Add(rankBG04);
            returnValue.Add(rankIcon04);
            returnValue.Add(GetRankTier(rankRange[3], rankRange[4], rank));
            returnValue.Add(transparent);
            returnValue.Add(transparent);
        }
        else if (rank < rankRange[5])
        {
            returnValue.Add(rankBG05);
            returnValue.Add(rankIcon05);
            returnValue.Add(transparent);
            returnValue.Add(GetRankTier(rankRange[4], rankRange[5], rank));
            returnValue.Add(transparent);
        }
        else if (rank < rankRange[6])
        {
            returnValue.Add(rankBG06);
            returnValue.Add(rankIcon06);
            returnValue.Add(transparent);
            returnValue.Add(GetRankTier(rankRange[5], rankRange[6], rank));
            returnValue.Add(transparent);
        }
        else if (rank < rankRange[7])
        {
            returnValue.Add(rankBG07);
            returnValue.Add(rankIcon07);
            returnValue.Add(transparent);
            returnValue.Add(transparent);
            returnValue.Add(GetRankTier(rankRange[6], rankRange[7], rank));
        }
        else
        {
            returnValue.Add(rankBG08);
            returnValue.Add(rankIcon08);
            returnValue.Add(transparent);
            returnValue.Add(transparent);
            returnValue.Add(transparent);
        }
        return returnValue;
    }

    Sprite GetRankTier(int rankStart, int rankEnd, int rank)
    {
        if(rank > rankEnd)
            return rankTier05;
        if (rank < rankStart)
            return rankTier01;

        int rangeLength = rankEnd - rankStart;
        int segmentSize = rangeLength / 5;
        int tier = (int)Math.Floor((double)(rank - rankStart) / segmentSize);

        switch (tier)
        {
            case 0:
                return rankTier01;
            case 1:
                return rankTier02;
            case 2:
                return rankTier03;
            case 3:
                return rankTier04;
            case 4:
                return rankTier05;
            default:
                return rankTier01;
        }
    }


    public Sprite GetChainNumSprite(int num)
    {
        switch (num)
        {
            case 0:
                return chainNumSet0;
            case 1:
                return chainNumSet1;
            case 2:
                return chainNumSet2;
            case 3:
                return chainNumSet3;
            case 4:
                return chainNumSet4;
            case 5:
                return chainNumSet5;
            case 6:
                return chainNumSet6;
            case 7:
                return chainNumSet7;
            case 8:
                return chainNumSet8;
            case 9:
                return chainNumSet9;
            default:
                return chainNumSet0;
        }
    }


}
