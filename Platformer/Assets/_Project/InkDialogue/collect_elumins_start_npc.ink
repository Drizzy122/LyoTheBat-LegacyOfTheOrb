=== collectEluminsStart ===
{ CollectEluminsQuestState :
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
    There are Bugs and Plague bats in the abandoned island can you clear that area please
* [Of Course]
        ~ StartQuest(CollectEluminsQuestId)
    Perfect thank you so much!
    
* [Not right now.]
    No worries. Come back if you change your mind.
--> END


= inProgress
Those bugs and plague bats destroyed that island, its a shame really it is said that there is a powerful orb somewhere within the island that can give you incredible power?
-> END



= canFinish
You defeated all enemies? Incredible—thank you!

* [no worries.]
    ~FinishQuest(CollectEluminsQuestId)
    you are the best, return to me soon and ill have another mission for you.
-> END


= finished
Thanks again for your help!
-> END



= default
Hm? What do you want?
*[Nothing, I guess.]
-> END


* { CollectEluminsQuestState == "CAN_FINISH"} [I have defeated all enemies you have requested.]
    ~ FinishQuest(CollectEluminsQuestId)
    Oh wow incredible thank you so much!
-> END