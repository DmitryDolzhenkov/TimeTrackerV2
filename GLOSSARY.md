# TimeTracker

A personal time tracker: one simple timer per user, with no projects, tasks, or categories.

## Timer

**Timer**:
The single time counter a user starts and stops; it has exactly one Timer State at any moment.
_Avoid_: Stopwatch, tracker

**Timer State**:
One of Stop, Active, Idle, Lock, or Shutdown.
_Avoid_: Status, mode

**Stop**:
The Timer State in which the user has not started the Timer or has stopped it; every launch of the application begins here.
_Avoid_: Paused, off

**Active**:
The Timer State in which the Timer is running and the user is present.
_Avoid_: Running, started

**Idle**:
The Timer State entered when a running Timer sees no keyboard or mouse input for longer than the Inactivity Threshold.
_Avoid_: Away, inactive, AFK

**Lock**:
The Timer State entered when a running Timer loses the user's workstation: it was locked, went to sleep or hibernation, was disconnected remotely, or was switched to another user.
_Avoid_: Blocked, suspended

**Shutdown**:
The Timer State recorded when the computer turns off or the user signs out while the Timer is not in Stop.
_Avoid_: Power off, exit

**Inactivity Threshold**:
The 15 minutes without input after which a running Timer enters Idle.
_Avoid_: Timeout, idle limit

**Transition**:
A change from one Timer State to another, together with when and why it happened.
_Avoid_: Status change, event

**Recovered Shutdown**:
A Shutdown added on the next launch because the application ended without recording one, such as after a power loss.
_Avoid_: Crash, inferred stop

## Time accounting

**Daily Session**:
All Counted Time and Adjustments within one local calendar day; starting, stopping, or restarting does not begin a new one.
_Avoid_: Session, workday, shift

**Counted Time**:
The time the Timer spends in Active, including the Inactivity Threshold that precedes Idle, before Adjustments.
_Avoid_: Elapsed time, duration

**Adjustment**:
A number of minutes the user manually adds to or removes from the current Daily Session.
_Avoid_: Correction, edit, manual time

## History

**Timer Log**:
The append-only chronological record of Transitions and Adjustments.
_Avoid_: History, audit log, event log
