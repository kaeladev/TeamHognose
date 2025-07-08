Holy molly, is the week over already?

* Time to head to the break room...
 -> finalday
* [Reflect]
I can't believe this is over.
Even if I didn't learn about baking, I still learnt from my new friends.
They taught me about friendship and plushie culture.
What a great internship experience!
-> finalday

== finalday== 
HELLO!!!! #all
omg i can't believe this is happening #squill
beware i WILL cry!!!! #squill
MAXIMUM CRYING!!!! #soup
A very strange day indeed... #tort
Well hello! First and foremost, I want to thank you for your time and energy. #inky
You really brought a lot of life to the Plûsh Brûlée bakery. #inky
ROCKSTAR BABYYY!!!! #soup
I suggest we have a big round of applause for our intern!! #inky
[Everybody applauds the best they can, even though some lack the limbs to do so] #all
HIP HIP HIP!!! #squill
HOORAY!!!! #squill
WELL PLAYED BUDDY!!! #soup
A CLOSING SPEECH NOW!!! #soup
CLOSING SPEECH!! CLOSING SPEECH!!! #all
* I had the best time at the bakery. Thank you so much for welcoming me!
* I'm very glad for all the plushies I met here. You will remain in my heart forever.
* My week of internship may be over, but I promise to visit!
- WOO-HOOOO!!!! #all
[Everybody gives you another round of applause] #all
Now, now. There is a tradition to celebrate the ends of internships. #inky
Right!!! #all
Oh yes, right. #tort
I want to challenge you to rock paper scissors. #tort
Do you accept?
* [Yes!]
-> rps
* [Hit me with your best shot!]
-> rps

== rps== 
VAR choice = ""
LIST options = (Rock),(Paper),(Scissor)
VAR result = ""

Okay so, close your eyes and make a choice.
 + [Rock]
    ~ choice = "Rock"
 + [Paper]
    ~ choice = "Paper"
 + [Scissor]
    ~ choice = "Scissor"

- You chose {choice} 
~ result = LIST_RANDOM(options)
and Tortilla chose {result}.
{
    - choice == "Rock" && result == Scissor:
        Oh, well played, you beat me! Very lucky! Good omen for the future. #tort
    - choice == "Rock" && result == Paper:
        Oopsie I'm sorry, I guess I won this time! I will consider this a good omen. #tort
    - choice == "Scissor" && result == Rock:
        Oopsie I'm sorry, I guess I won this time! I will consider this a good omen. #tort
    - choice == "Scissor" && result == Paper:
        Oh, well played, you beat me! Very lucky! Good omen for the future. #tort
    - choice == "Paper" && result == Rock:
        Oh, well played, you beat me! Very lucky! Good omen for the future. #tort
    - choice == "Paper" && result == Scissor:
        Oopsie I'm sorry, I guess I won this time! I will consider this a good omen. #tort
    - else:
        Hihi, I guess we both got lucky then hehe. We both lost... Or we both won! I think it means we will meet again in the future. #tort
}

Please come back. #tort
We will miss you. #tort
do not forget about us!!!! #squill
BEST BUDS FOR LIFE RIGHT!? #soup
MAXIMUM TEARS!!!! #soup
before we all come home... #squill
i wanted to let y'all know that tomorrow i'll organize a party at the beach!! #squill
i'd love for y'all to come!!! #squill
besties hang-out!!! #squill
Thank you Squilliam. #inky
And thank you, my friend. We had a great time together. #inky
Let's all look forward a nice week-end! #inky
And perhaps meeting again, if you will. #tort
You will always be welcome at the Plûsh Brûlée. #inky
LA CRÈME DE LA CRÈME!!! #soup
I will never forget my time here.
Thank you so much.
I will be forever grateful for this time.
That is very sweet of you. Thank you. #tort
All right then... I guess that's a wrap. #Inky
From the bottom of our hearts, many thanks! #all
AND HAVE A NICE WEEK-END!!! #all
PLÛSH BRÛLÉE - AWAY!!! #all
-> END