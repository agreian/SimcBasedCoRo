using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using CommonBehaviors.Actions;
using SimcBasedCoRo.ClassSpecific;
using SimcBasedCoRo.Extensions;
using SimcBasedCoRo.Managers;
using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.TreeSharp;
using Styx.WoWInternals;
using Styx.WoWInternals.World;
using Styx.WoWInternals.WoWObjects;
using Action = Styx.TreeSharp.Action;

namespace SimcBasedCoRo.Utilities
{
    public static class Spell
    {
        #region Fields

        internal static readonly Dictionary<string, long> UndefinedSpells = new Dictionary<string, long>();

        private const int SAME_SPELL_THROTTLE = 450;

        private static readonly Dictionary<string, DateTime> _doubleCastPreventionDict = new Dictionary<string, DateTime>();
        private static readonly SpellFindResults _emptySfr = new SpellFindResults(null);

        private static bool _useAoe = true;
        private static bool _useAutoKick = true;
        private static bool _useCooldown = true;
        private static bool _useDefensiveCooldown = true;

        #endregion

        #region Properties

        public static bool GcdActive
        {
            get { return SpellManager.GlobalCooldown; }
        }

        public static TimeSpan GcdTimeLeft
        {
            get { return SpellManager.GlobalCooldownLeft; }
        }

        public static WoWSpell GetPendingCursorSpell
        {
            get { return Me.CurrentPendingCursorSpell; }
        }

        public static LocalPlayer Me
        {
            get { return StyxWoW.Me; }
        }

        public static bool UseAoe
        {
            get { return _useAoe; }
            set { _useAoe = value; }
        }

        public static bool UseAutoKick
        {
            get { return _useAutoKick; }
            set { _useAutoKick = value; }
        }

        public static bool UseCooldown
        {
            get { return _useCooldown; }
            set { _useCooldown = value; }
        }

        public static bool UseDefensiveCooldown
        {
            get { return _useDefensiveCooldown; }
            set { _useDefensiveCooldown = value; }
        }

        internal static string LastSpellCast { get; set; }
        internal static WoWGuid LastSpellTarget { get; set; }

        private static string BuffName { get; set; }
        private static WoWUnit BuffUnit { get; set; }

        #endregion

        #region Public Methods

        public static Composite Buff(string name, SimpleBooleanDelegate requirements)
        {
            return Buff(name, false, on => StyxWoW.Me.CurrentTarget, requirements, name);
        }

        public static Composite Buff(string name, UnitSelectionDelegate onUnit, SimpleBooleanDelegate requirements)
        {
            return Buff(name, false, onUnit, requirements);
        }

        public static Composite Buff(string name, int expirSecs, UnitSelectionDelegate onUnit = null, SimpleBooleanDelegate require = null, bool myBuff = true, params string[] buffNames)
        {
            return Buff(sp => name, expirSecs, onUnit, require, myBuff, HasGcd.Yes, buffNames);
        }

        public static Composite BuffSelf(string name, SimpleBooleanDelegate requirements)
        {
            return Buff(name, false, on => Me, requirements);
        }

        public static Composite BuffSelf(string name, SimpleBooleanDelegate requirements, HasGcd gcd)
        {
            return Buff(b => name, 0, on => Me, requirements, false, gcd);
        }

        public static Composite BuffSelfAndWait(string name, SimpleBooleanDelegate requirements = null, int expirSecs = 0, CanRunDecoratorDelegate until = null, bool measure = false, HasGcd gcd = HasGcd.Yes)
        {
            return BuffSelfAndWait(b => name, requirements, expirSecs, until, measure, gcd);
        }

        public static bool CanCastHack(string castName, WoWUnit unit, bool skipWowCheck = false)
        {
            SpellFindResults sfr;
            if (!SpellManager.FindSpell(castName, out sfr))
            {
                // Logger.WriteDebug("CanCast: spell [{0}] not known", castName);
                AddUndefinedSpell(castName);
                return false;
            }

            return CanCastHack(sfr, unit, skipWowCheck);
        }

        public static Composite Cast(string name, SimpleBooleanDelegate requirements)
        {
            return Cast(sp => name, requirements);
        }

        public static Composite Cast(string name, UnitSelectionDelegate onUnit, SimpleBooleanDelegate requirements)
        {
            return Cast(sp => name, onUnit, requirements);
        }

        public static Composite Cast(string name, UnitSelectionDelegate onUnit)
        {
            return Cast(sp => name, onUnit);
        }

        public static Composite Cast(SimpleStringDelegate name, UnitSelectionDelegate onUnit, SimpleBooleanDelegate requirements)
        {
            return Cast(name, ret => true, onUnit, requirements);
        }

        public static Composite CastOnGround(string spellName, UnitSelectionDelegate onUnit, SimpleBooleanDelegate requirements, bool waitForSpell = true)
        {
            if (spellName == null || onUnit == null || requirements == null)
                return new ActionAlwaysFail();

            SpellFindDelegate ssd =
                (object ctx, out SpellFindResults sfr) =>
                {
                    if (!SpellManager.FindSpell(spellName, out sfr))
                    {
                        AddUndefinedSpell(spellName);
                        return false;
                    }
                    return true;
                };

            return new PrioritySelector(
                ctx =>
                {
                    var cc = new CastContext(ctx, ssd, onUnit);
                    var cog = new CogContext(cc, desc => string.Format("on {0} @ {1:F1}%", cc.Unit.Name, cc.Unit.HealthPercent));
                    return cog;
                },
                ContextCastOnGround(requirements, waitForSpell)
                );
        }

        public static int GetCharges(string name)
        {
            SpellFindResults sfr;
            if (SpellManager.FindSpell(name, out sfr))
            {
                WoWSpell spell = sfr.Override ?? sfr.Original;
                return spell.GetCharges();
            }
            return 0;
        }

        public static TimeSpan GetSpellCastTime(string spellName)
        {
            SpellFindResults sfr;
            if (SpellManager.FindSpell(spellName, out sfr))
            {
                var spell = sfr.Override ?? sfr.Original;
                return GetSpellCastTime(spell);
            }

            return TimeSpan.Zero;
        }

        public static int GetSpellCharges(string spellName)
        {
            SpellFindResults sfr;
            if (SpellManager.FindSpell(spellName, out sfr))
            {
                var spell = sfr.Override ?? sfr.Original;
                return spell.GetCharges();
            }

            return 0;
        }

        public static TimeSpan GetSpellCooldown(string spellName)
        {
            SpellFindResults sfr;
            if (SpellManager.FindSpell(spellName, out sfr)) return (sfr.Override ?? sfr.Original).CooldownTimeLeft;

            return TimeSpan.MaxValue;
        }

        public static bool IsGlobalCooldown()
        {
            return SpellManager.GlobalCooldown;
        }

        public static bool IsSpellOnCooldown(string castName)
        {
            SpellFindResults sfr;
            if (!SpellManager.FindSpell(castName, out sfr))
                return true;

            WoWSpell spell = sfr.Override ?? sfr.Original;
            return IsSpellOnCooldown(spell);
        }

        public static bool IsSpellOnCooldown(WoWSpell spell)
        {
            if (spell == null)
                return true;

            if (Me.ChanneledCastingSpellId != 0)
                return true;

            var num = SimCraftCombatRoutine.Latency * 2u;
            if (StyxWoW.Me.IsCasting && Me.CurrentCastTimeLeft.TotalMilliseconds > num)
                return true;

            if (spell.CooldownTimeLeft.TotalMilliseconds > num)
                return true;

            return false;
        }

        #endregion

        #region Private Methods

        private static void AddUndefinedSpell(string spellName)
        {
            if (UndefinedSpells.ContainsKey(spellName))
                UndefinedSpells[spellName] = UndefinedSpells[spellName] + 1;
            else
                UndefinedSpells.Add(spellName, 1);
        }

        private static bool AllowMovingWhileCasting(WoWSpell spell)
        {
            // quick return for instant spells
            if (spell.IsInstantCast() && !spell.IsChanneled)
            {
                return true;
            }

            // assume we cant move, but check for class specific buffs which allow movement while casting
            var allowMovingWhileCasting = false;
            if (Me.Class == WoWClass.Shaman)
                allowMovingWhileCasting = spell.Name == "Lightning Bolt";
            else if (Me.Specialization == WoWSpec.MageFire)
                allowMovingWhileCasting = spell.Name == "Scorch";
            else if (Me.Class == WoWClass.Hunter)
                allowMovingWhileCasting = spell.Name == "Steady Shot" || (spell.Name == "Aimed Shot" && TalentManager.HasGlyph("Aimed Shot")) || spell.Name == "Cobra Shot";
            else if (Me.Class == WoWClass.Warlock)
                allowMovingWhileCasting = (spell.Name == "Incinerate" || spell.Name == "Malefic Grasp" || spell.Name == "Shadow Bolt") && Warlock.talent.kiljaedens_cunning.enabled;

            if (!allowMovingWhileCasting)
            {
                allowMovingWhileCasting = HaveAllowMovingWhileCastingAura(spell);

                // we will atleast check spell cooldown... we may still end up wasting buff, but this reduces the chance
                if (!allowMovingWhileCasting && spell.CooldownTimeLeft == TimeSpan.Zero)
                {
                    var castSuccess = CastBuffToAllowCastingWhileMoving();
                    if (castSuccess)
                        allowMovingWhileCasting = HaveAllowMovingWhileCastingAura();
                }
            }

            return allowMovingWhileCasting;
        }

        private static Composite Buff(string name, bool myBuff, UnitSelectionDelegate onUnit, SimpleBooleanDelegate requirements)
        {
            return Buff(name, myBuff, onUnit, requirements, name);
        }

        private static Composite Buff(string name, bool myBuff, UnitSelectionDelegate onUnit, SimpleBooleanDelegate requirements, params string[] buffNames)
        {
            return Buff(sp => name, myBuff, onUnit, requirements, buffNames);
        }

        private static Composite Buff(SimpleStringDelegate name, bool myBuff, UnitSelectionDelegate onUnit, SimpleBooleanDelegate require, params string[] buffNames)
        {
            return new Decorator(
                ret =>
                {
                    if (onUnit == null || name == null || require == null)
                        return false;

                    BuffUnit = onUnit(ret);
                    if (BuffUnit == null)
                        return false;

                    BuffName = name(ret);
                    if (BuffName == null)
                        return false;

                    SpellFindResults sfr;
                    if (!SpellManager.FindSpell(BuffName, out sfr))
                        return false;

                    if (sfr.Override != null)
                        BuffName = sfr.Override.Name;

                    if (DoubleCastContains(BuffUnit, BuffName))
                        return false;

                    if (!buffNames.Any())
                        return !(myBuff ? BuffUnit.HasMyAura(BuffName) : BuffUnit.HasAura(BuffName));

                    bool buffFound;
                    try
                    {
                        buffFound = myBuff ? buffNames.Any(b => BuffUnit.HasMyAura(b)) : buffNames.Any(b => BuffUnit.HasAura(b));
                    }
                    catch
                    {
                        // mark as found buff, so we return false
                        buffFound = true;
                    }

                    return !buffFound;
                },
                new Sequence(
                    // new Action(ctx => _lastBuffCast = name),
                    Cast(sp => BuffName, chkMov => true, on => BuffUnit, require, cancel => false /* causes cast to complete */),
                    new Action(ret => UpdateDoubleCast(BuffName, BuffUnit))
                    )
                );
        }

        private static Composite Buff(SimpleStringDelegate name, int expirSecs, UnitSelectionDelegate onUnit = null, SimpleBooleanDelegate require = null, bool myBuff = true, HasGcd gcd = HasGcd.Yes, params string[] buffNames)
        {
            return Buff(name, TimeSpan.FromSeconds(expirSecs), onUnit, require, myBuff, gcd, buffNames);
        }

        private static Composite Buff(SimpleStringDelegate name, TimeSpan expires, UnitSelectionDelegate onUnit = null, SimpleBooleanDelegate require = null, bool myBuff = true, HasGcd gcd = HasGcd.Yes, params string[] buffNames)
        {
            if (onUnit == null)
                onUnit = u => Me.CurrentTarget;
            if (require == null)
                require = req => true;

            return new Decorator(
                ret =>
                {
                    if (onUnit == null || name == null || require == null)
                        return false;

                    BuffUnit = onUnit(ret);
                    if (BuffUnit == null)
                        return false;

                    BuffName = name(ret);
                    if (BuffName == null)
                        return false;

                    SpellFindResults sfr;
                    if (!SpellManager.FindSpell(BuffName, out sfr))
                    {
                        AddUndefinedSpell(BuffName);
                        return false;
                    }

                    var spell = sfr.Override ?? sfr.Original;
                    BuffName = spell.Name;
                    if (DoubleCastContains(BuffUnit, BuffName))
                        return false;

                    if (!spell.CanCast && (sfr.Override == null || !sfr.Original.CanCast))
                    {
                        return false;
                    }

                    bool hasExpired;
                    if (!buffNames.Any())
                    {
                        hasExpired = BuffUnit.HasAuraExpired(BuffName, expires, myBuff);

                        return hasExpired;
                    }

                    hasExpired = SpellManager.HasSpell(BuffName) && buffNames.All(b => BuffUnit.HasKnownAuraExpired(b, expires, myBuff));

                    return hasExpired;
                },
                new Sequence(
                    // new Action(ctx => _lastBuffCast = name),
                    Cast(sp => BuffName, chkMov => true, onUnit, require, cancel => false /* causes cast to complete */, gcd: gcd),
                    new Action(ret => UpdateDoubleCast(BuffName, BuffUnit))
                    )
                );
        }

        private static Composite BuffSelf(SimpleStringDelegate name, SimpleBooleanDelegate requirements, int expirSecs, HasGcd gcd = HasGcd.Yes)
        {
            return Buff(name, expirSecs, on => Me, requirements, gcd: gcd);
        }

        private static Composite BuffSelfAndWait(SimpleStringDelegate name, SimpleBooleanDelegate requirements = null, int expirSecs = 0, CanRunDecoratorDelegate until = null, bool measure = false, HasGcd gcd = HasGcd.Yes)
        {
            if (requirements == null)
                requirements = req => true;

            if (until == null)
                until = u => StyxWoW.Me.HasAura(name(u));

            return new Sequence(
                BuffSelf(name, requirements, expirSecs, gcd),
                new PrioritySelector(
                    new DynaWait(
                        time => TimeSpan.FromMilliseconds(Me.Combat ? 500 : 1000),
                        until,
                        new ActionAlwaysSucceed(),
                        measure
                        ),
                    new Action(r => RunStatus.Failure)
                    )
                );
        }

        private static bool CanCastHack(SpellFindResults sfr, WoWUnit unit, bool skipWowCheck = false)
        {
            var spell = sfr.Override ?? sfr.Original;

            // check range
            if (!CanCastHackInRange(spell, unit))
                return false;

            // check if movement prevents cast
            if (CanCastHackWillOurMovementInterrupt(spell))
                return false;

            if (CanCastHackIsCastInProgress(spell))
                return false;

            if (!CanCastHackHaveEnoughPower(spell))
                return false;

            // override spell will sometimes always have cancast=false, so check original also
            if (!skipWowCheck && !spell.CanCast && (sfr.Override == null || !sfr.Original.CanCast))
            {
                return false;
            }

            return true;
        }

        private static bool CanCastHackHaveEnoughPower(WoWSpell spell)
        {
            if (!spell.CanCast)
            {
                return false;
            }

            return true;
        }

        private static bool CanCastHackInRange(WoWSpell spell, WoWUnit unit)
        {
            if (unit != null && !spell.IsSelfOnlySpell && !unit.IsMe)
            {
                {
                    if (spell.IsMeleeSpell && !unit.IsWithinMeleeRange)
                    {
                        return false;
                    }
                    if (spell.HasRange)
                    {
                        if (unit.Distance > spell.ActualMaxRange(unit))
                        {
                            return false;
                        }
                        if (unit.Distance < spell.ActualMinRange(unit))
                        {
                            return false;
                        }
                    }
                }

                if (!unit.InLineOfSpellSight)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool CanCastHackIsCastInProgress(WoWSpell spell)
        {
            var lat = SimCraftCombatRoutine.Latency * 2u;

            if (Me.ChanneledCastingSpellId == 0)
            {
                if (StyxWoW.Me.IsCasting && Me.CurrentCastTimeLeft.TotalMilliseconds > lat)
                {
                    return true;
                }
            }

            if (spell.CooldownTimeLeft.TotalMilliseconds > lat)
            {
                return true;
            }

            return false;
        }

        private static bool CanCastHackWillOurMovementInterrupt(WoWSpell spell)
        {
            if ((spell.CastTime != 0u || spell.IsChanneled) && Me.IsMoving && !AllowMovingWhileCasting(spell))
            {
                return true;
            }

            return false;
        }

        private static Composite Cast(SimpleStringDelegate name, UnitSelectionDelegate onUnit)
        {
            return Cast(name, onUnit, req => true);
        }

        private static Composite Cast(SimpleStringDelegate name, SimpleBooleanDelegate requirements)
        {
            return Cast(name, onUnit => StyxWoW.Me.CurrentTarget, requirements);
        }

        private static Composite Cast(SimpleStringDelegate name, SimpleBooleanDelegate checkMovement, UnitSelectionDelegate onUnit, SimpleBooleanDelegate requirements, SimpleBooleanDelegate cancel = null, LagTolerance allow = LagTolerance.Yes,
            bool skipWowCheck = false, CanCastDelegate canCast = null, HasGcd gcd = HasGcd.Yes)
        {
            SpellFindDelegate ssd =
                (object ctx, out SpellFindResults sfr) =>
                {
                    if (name != null)
                    {
                        var spellName = name(ctx);
                        if (spellName != null)
                        {
                            return SpellManager.FindSpell(spellName, out sfr);
                        }
                    }

                    sfr = _emptySfr;
                    return false;
                };

            return Cast(ssd, checkMovement, onUnit, requirements, cancel, allow, skipWowCheck, canCast, gcd);
        }

        private static Composite Cast(SpellFindDelegate ssd, SimpleBooleanDelegate checkMovement, UnitSelectionDelegate onUnit, SimpleBooleanDelegate requirements, SimpleBooleanDelegate cancel = null, LagTolerance allow = LagTolerance.Yes,
            bool skipWowCheck = false, CanCastDelegate canCast = null, HasGcd gcd = HasGcd.Yes)
        {
            // only need to check these at creation time
            if (ssd == null || checkMovement == null || onUnit == null || requirements == null)
                return new ActionAlwaysFail();

            if (canCast == null)
                canCast = CanCastHack;

            Composite comp = new PrioritySelector(
                // create a CastContext object to save passed in context and other values
                ctx => new CastContext(ctx, ssd, onUnit),
                new Sequence(
                    // cast the spell, saving state information including if we queued this cast
                    new Action(ret =>
                    {
                        var cctx = ret.CastContext();

                        if (cctx.Spell == null)
                            return RunStatus.Failure;

                        if (cctx.Unit == null)
                            return RunStatus.Failure;

                        if (!requirements(cctx.Context))
                            return RunStatus.Failure;

                        if (checkMovement(cctx.Context) && Me.IsMoving && !AllowMovingWhileCasting(cctx.Spell))
                        {
                            return RunStatus.Failure;
                        }

                        // check we can cast it on target without checking for movement
                        // if (!Spell.CanCastHack(_spell, cctx.unit, true, false, allow == LagTolerance.Yes))
                        if (!canCast(cctx.Sfr, cctx.Unit, skipWowCheck))
                        {
                            return RunStatus.Failure;
                        }

                        // save status of queueing spell (lag tolerance - the prior spell still completing)
                        cctx.IsSpellBeingQueued = allow == LagTolerance.Yes && (GcdActive || StyxWoW.Me.IsCasting || StyxWoW.Me.IsChanneling);

                        const int penance = 047540;
                        LogCast(
                            cctx.Spell.Name,
                            cctx.Unit,
                            cctx.Health,
                            cctx.Distance,
                            cctx.Spell.IsHeal() || (cctx.Spell.Id == penance && cctx.Unit.IsFriendly)
                            );

                        if (!CastPrimative(cctx.Spell, cctx.Unit))
                        {
                            Logging.Write(Colors.LightPink, "cast of {0} on {1} failed!", cctx.Spell.Name, cctx.Unit.Name);
                            return RunStatus.Failure;
                        }

                        return RunStatus.Success;
                    }),
                    new Action(r => RunStatus.Success),

                    // for instant spell, wait for GCD to start
                    // for non-instant spell, wait for .IsCasting / .IsChanneling to start
                    new PrioritySelector(
                        new Wait(
                            TimeSpan.FromMilliseconds(350),
                            until =>
                            {
                                CastContext cctx = until.CastContext();
                                if (gcd == HasGcd.No)
                                {
                                    return true;
                                }

                                if (cctx.Spell.IsInstantCast() && GcdTimeLeft.TotalMilliseconds > 650)
                                {
                                    return true;
                                }

                                if (Me.CurrentCastTimeLeft.TotalMilliseconds > 750)
                                {
                                    return true;
                                }

                                if (Me.CurrentChannelTimeLeft.TotalMilliseconds > 750)
                                {
                                    return true;
                                }

                                return false;
                            },
                            new ActionAlwaysSucceed()
                            ),
                        new Action(r => RunStatus.Success)
                        ),

                    // now check for one of the possible done casting states
                    new PrioritySelector(
                        // for cast already ended, assume it has no Global Cooldown
                        new Decorator(
                            ret => !IsGlobalCooldown() && !IsCastingOrChannelling(),
                            new Action(r => RunStatus.Success)
                            ),

                        // for instant or no cancel method given, we are done
                        new Decorator(
                            ret => gcd == HasGcd.No || cancel == null || ret.CastContext().Spell.IsInstantCast(),
                            new Action(r => RunStatus.Success)
                            ),

                        // while casting/channeling call the cancel method to see if we should abort
                        new Wait(12,
                            until =>
                            {
                                CastContext cctx = until.CastContext();

                                // Interrupted or finished casting. 
                                if (!IsCastingOrChannelling(allow))
                                {
                                    return true;
                                }

                                // check cancel delegate if we are finished
                                if (cancel != null && cancel(cctx.Context))
                                {
                                    SpellManager.StopCasting();
                                    Logging.Write(LogColor.Cancel, "/cancel {0} on {1} @ {2:F1}%", cctx.Spell.Name, cctx.Unit.Name, cctx.Unit.HealthPercent);
                                    return true;
                                }
                                // continue casting/channeling at this point
                                return false;
                            },
                            new ActionAlwaysSucceed()
                            ),

                        // if we are here, we timed out after 12 seconds (very odd)
                        new Action(r => RunStatus.Success)
                        ),

                    // made it this far then we are RunStatus.Success, so reset wowunit reference and return
                    new Action(ret =>
                    {
                        CastContext cctx = ret.CastContext();
                        cctx.Unit = null;
                        cctx.Spell = null;
                        return RunStatus.Success;
                    })
                    ),

                // cast Sequence failed, so only thing left is to reset cached references and report failure
                new Action(ret =>
                {
                    CastContext cctx = ret.CastContext();
                    cctx.Unit = null;
                    cctx.Spell = null;
                    return RunStatus.Failure;
                })
                );

            // when no cancel method in place, we will return immediately so.....
            // .. throttle attempts at casting this spell.  note: this only limits this 
            // .. instance of the spell.cast behavior.  in other words, if this is for a cast
            // .. of flame shock, it would only throttle this behavior tree instance, not any 
            // .. other trees which also call Spell.Cast("flame shock")
            if (cancel == null)
                comp = new Throttle(SAME_SPELL_THROTTLE, comp);

            return comp;
        }

        private static bool CastBuffToAllowCastingWhileMoving()
        {
            string spell = null;
            var allowMovingWhileCasting = false;

            if (Me.Class == WoWClass.Shaman)
                spell = "Spiritwalker's Grace";
            else if (Me.Class == WoWClass.Mage)
                spell = "Ice Floes";

            if (spell != null)
            {
                if (DoubleCastContains(Me, spell))
                    return false;

                // DumpDoubleCast();

                if (CanCastHack(spell, Me)) // Spell.CanCastHack(spell, Me))
                {
                    LogCast(spell, Me);
                    allowMovingWhileCasting = CastPrimative(spell, Me);
                    if (allowMovingWhileCasting)
                        UpdateDoubleCast(spell, Me, 1);
                }
            }

            return allowMovingWhileCasting;
        }

        private static CastContext CastContext(this object ctx)
        {
            return (CastContext) ctx;
        }

        private static bool CastPrimative(WoWSpell spell)
        {
            LastSpellCast = spell == null ? string.Empty : spell.Name;
            LastSpellTarget = WoWGuid.Empty;
            return SpellManager.Cast(spell);
        }

        private static bool CastPrimative(WoWSpell spell, WoWUnit unit)
        {
            LastSpellCast = spell == null ? string.Empty : spell.Name;
            LastSpellTarget = unit == null ? WoWGuid.Empty : unit.Guid;
            return SpellManager.Cast(spell, unit);
        }

        private static bool CastPrimative(string spellName, WoWUnit unit)
        {
            LastSpellCast = spellName ?? string.Empty;
            LastSpellTarget = unit == null ? WoWGuid.Empty : unit.Guid;
            return SpellManager.Cast(spellName, unit);
        }

        private static CogContext CogContext(this object ctx)
        {
            return (CogContext) ctx;
        }

        private static Composite ContextCastOnGround(SimpleBooleanDelegate reqd, bool waitForSpell = true)
        {
            var requirements = reqd ?? (r => true);
            return new Decorator(
                req =>
                {
                    CogContext cog = req.CogContext();
                    if (cog.Spell == null || cog.Loc == WoWPoint.Empty || !requirements(cog.Context))
                        return false;

                    if (!CanCastHack(cog.Sfr, null, skipWowCheck: true))
                        return false;

                    if (!LocationInRange(cog.Name, cog.Loc))
                        return false;

                    if (!GameWorld.IsInLineOfSpellSight(StyxWoW.Me.GetTraceLinePos(), cog.Loc))
                        return false;

                    return true;
                },
                new Sequence(
                    // if wait requested, wait for spell in progress to be clear
                    new DecoratorContinue(
                        req => waitForSpell,
                        new PrioritySelector(
                            new Wait(
                                TimeSpan.FromMilliseconds(500),
                                until => !IsGlobalCooldown() && !IsCastingOrChannelling(),
                                new ActionAlwaysSucceed()
                                ),
                            new Action(r => RunStatus.Failure)
                            )
                        ),

                    // cast spell which needs ground targeting
                    new Action(ret =>
                    {
                        CogContext cog = ret.CogContext();
                        Logging.Write(cog.Spell.IsHeal() ? LogColor.SpellHeal : LogColor.SpellNonHeal, "*{0} {1}at {2:F1} yds {3}", cog.Name, cog.TargetDesc, cog.Loc.Distance(StyxWoW.Me.Location), cog.Loc);
                        return CastPrimative(cog.Spell) ? RunStatus.Success : RunStatus.Failure;
                    }),

                    // confirm spell is on cursor requiring targeting
                    new PrioritySelector(
                        new WaitContinue(
                            1,
                            until => GetPendingCursorSpell != null && GetPendingCursorSpell.Name == until.CogContext().Name,
                            new ActionAlwaysSucceed()
                            ),
                        new Action(r =>
                        {
                            Lua.DoString("SpellStopTargeting()"); // shouldn't be needed, but handle GetPendingCursorSpell breakage
                            return RunStatus.Failure;
                        })
                        ),

                    // click on ground
                    new Action(ret =>
                    {
                        if (!SpellManager.ClickRemoteLocation(ret.CogContext().Loc))
                        {
                            return RunStatus.Failure;
                        }

                        return RunStatus.Success;
                    }),

                    // handle waiting if requested
                    new PrioritySelector(
                        new Decorator(
                            req => !waitForSpell,
                            new ActionAlwaysSucceed()
                            ),
                        new Wait(
                            TimeSpan.FromMilliseconds(500),
                            until => IsGlobalCooldown() || IsCastingOrChannelling() || Me.CurrentPendingCursorSpell == null,
                            new ActionAlwaysSucceed()
                            ),
                        new Action(ret =>
                        {
                            // Pending Spell Cursor API is broken... seems like we can't really check at this point, so assume it failed and worked... uggghhh
                            Lua.DoString("SpellStopTargeting()");
                            return RunStatus.Failure;
                        })
                        ),

                    // confirm we are done with cast
                    new PrioritySelector(
                        new Decorator(
                            ret => !waitForSpell,
                            new ActionAlwaysSucceed()
                            ),
                        new Wait(
                            TimeSpan.FromMilliseconds(750),
                            until => IsGlobalCooldown() || IsCastingOrChannelling(),
                            new ActionAlwaysSucceed()
                            ),
                        new ActionAlwaysSucceed()
                        )
                    )
                );
        }

        private static bool DoubleCastContains(WoWUnit unit, string spellName)
        {
            return _doubleCastPreventionDict.ContainsKey(DoubleCastKey(unit, spellName));
        }

        private static string DoubleCastKey(WoWGuid guid, string spellName)
        {
            return guid + "-" + spellName;
        }

        private static string DoubleCastKey(WoWUnit unit, string spell)
        {
            return DoubleCastKey(unit.Guid, spell);
        }

        private static TimeSpan GetSpellCastTime(WoWSpell spell)
        {
            if (spell == null) return TimeSpan.Zero;

            var time = (int) spell.CastTime;
            if (time == 0) time = spell.BaseDuration;
            return TimeSpan.FromMilliseconds(time);
        }

        private static bool HaveAllowMovingWhileCastingAura(WoWSpell spell = null)
        {
            var found = Me.GetAllAuras().FirstOrDefault(a => a.ApplyAuraType == (WoWApplyAuraType) 330 && (spell == null || GetSpellCastTime(spell) < a.TimeLeft));

            return found != null;
        }

        private static bool IsCasting(LagTolerance allow = LagTolerance.Yes)
        {
            try
            {
                if (!StyxWoW.Me.IsCasting)
                    return false;
            }
            catch (InvalidObjectPointerException)
            {
                return true;
            }

            if (StyxWoW.Me.ChannelObjectGuid.IsValid)
                return false;

            var latency = SimCraftCombatRoutine.Latency * 2;
            if (allow == LagTolerance.Yes && StyxWoW.Me.CurrentCastTimeLeft.TotalMilliseconds < latency)
                return false;

            return true;
        }

        private static bool IsCastingOrChannelling(LagTolerance allow = LagTolerance.Yes)
        {
            return IsCasting(allow) || IsChannelling(allow);
        }

        private static bool IsChannelling(LagTolerance allow = LagTolerance.Yes)
        {
            try
            {
                if (!StyxWoW.Me.IsChanneling)
                    return false;
            }
            catch (InvalidObjectPointerException)
            {
                return true;
            }

            var latency = SimCraftCombatRoutine.Latency * 2;
            var timeLeft = StyxWoW.Me.CurrentChannelTimeLeft;
            if (allow == LagTolerance.Yes && timeLeft.TotalMilliseconds < latency)
                return false;

            return true;
        }

        private static bool LocationInRange(string spellName, WoWPoint loc)
        {
            if (loc != WoWPoint.Empty)
            {
                SpellFindResults sfr;
                if (SpellManager.FindSpell(spellName, out sfr))
                {
                    var spell = sfr.Override ?? sfr.Original;
                    if (spell.HasRange)
                    {
                        return spell.MinRange <= Me.Location.Distance(loc) && Me.Location.Distance(loc) < spell.MaxRange;
                    }
                }
            }

            return false;
        }

        private static void LogCast(string sname, WoWUnit unit, bool isHeal = false)
        {
            LogCast(sname, unit, unit.HealthPercent, unit.SpellDistance(), isHeal);
        }

        private static void LogCast(string sname, WoWUnit unit, double health, double dist, bool isHeal = false)
        {
            var clr = isHeal ? LogColor.SpellHeal : LogColor.SpellNonHeal;

            if (unit.IsMe)
                Logging.Write(clr, "*{0} on Me @ {1:F1}%", sname, health);
            else
                Logging.Write(clr, "*{0} on {1} @ {2:F1}% at {3:F1} yds", sname, unit.Name, health, dist);
        }

        private static void UpdateDoubleCast(string spellName, WoWUnit unit, int milliSecs = 3000)
        {
            if (unit == null)
                return;

            var expir = DateTime.UtcNow + TimeSpan.FromMilliseconds(milliSecs);
            var key = DoubleCastKey(unit.Guid, spellName);
            if (_doubleCastPreventionDict.ContainsKey(key))
                _doubleCastPreventionDict[key] = expir;
            else
                _doubleCastPreventionDict.Add(key, expir);
        }

        #endregion
    }
}