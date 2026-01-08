=== killEnemiesStart ===
{ KillEnemiesQuestState :
    - "REQUIREMENTS_NOT_MET": -> requirementsNotMet
    - "CAN_START": -> canStart
    - "IN_PROGRESS": -> inProgress
    - "CAN_FINISH": -> canFinish
    - "FINISHED": ->  finished
    - else: -> END
}
            
= requirementsNotMet
// not possible for this quest, but putting something here anyways
You’re not quite ready for this task. Come back after you’ve gained a bit more experience.
-> END



= canStart
Hey, traveler! Could I ask a favor?
* [What do you need help with?]
    There are enemies in the nearby island can you defeat them please
    ->END
* [Of Course]
        ~ StartQuest(KillEnemiesQuestId)
    Perfect thank you so much!
    
* [Not right now.]
    No worries. Come back if you change your mind.
--> END


= inProgress
When you defeat the enemies let me know please
-> END

= canFinish
You defeated all enemies? Incredible—thank you!

* [no worries.]
    ~FinishQuest(KillEnemiesQuestId)
Thanks
-> END


= finished
Thanks again for your help!
-> END


= default
Hm? What do you want?
*[Nothing, I guess.]
-> END


* { KillEnemiesQuestState == "CAN_FINISH"} [I have defeated all enemies you have requested.]
    ~ FinishQuest(KillEnemiesQuestId)
    Oh wow incredible thank you so much!
-> END