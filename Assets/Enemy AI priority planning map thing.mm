<map version="1.0.1">
<!-- To view this file, download free mind mapping software FreeMind from http://freemind.sourceforge.net -->
<node CREATED="1648096689028" ID="ID_1392374564" MODIFIED="1648096697924" TEXT="Enemy AI">
<node CREATED="1648097151702" FOLDED="true" ID="ID_1053982806" MODIFIED="1648536251372" POSITION="right">
<richcontent TYPE="NODE"><html>
  <head>
    
  </head>
  <body>
    <p>
      Avoid attack
    </p>
    <p>
      Prerequisites:
    </p>
    <p>
      Enemy is aware of an attack that is about to happen&#160;(check if a variable is not null)
    </p>
    <p>
      That attack is going to hit them (have some kind of InDanger() bool using the attack as a parameter)<br />
    </p>
  </body>
</html>
</richcontent>
<node CREATED="1648097906899" ID="ID_231580862" MODIFIED="1648097942820" TEXT="Move to safe position"/>
<node CREATED="1648097936950" ID="ID_617231548" MODIFIED="1648097961259" TEXT="Use defensive ability to block attack"/>
</node>
<node CREATED="1648096725671" ID="ID_1961056038" MODIFIED="1648536284084" POSITION="right" STYLE="fork">
<richcontent TYPE="NODE"><html>
  <head>
    
  </head>
  <body>
    <p>
      Eliminate target
    </p>
    <p>
      Prerequisites:
    </p>
    <p>
      Is the AI aware of a target?
    </p>
    <p>
      Does the AI know the target's position?
    </p>
    <p>
      If not, has the AI searched at the target's last known position?<br />
    </p>
  </body>
</html>
</richcontent>
<node CREATED="1648096747410" ID="ID_433525662" MODIFIED="1648536284083">
<richcontent TYPE="NODE"><html>
  <head>
    
  </head>
  <body>
    <p>
      Combat<br />(this can vary greatly depending on the enemy type)
    </p>
    <p>
      
    </p>
    <p>
      Prerequisites:
    </p>
    <p>
      AI is aware of the player's exact position
    </p>
  </body>
</html>
</richcontent>
<node CREATED="1648096758203" ID="ID_1403515245" MODIFIED="1648536284083" TEXT="Attack enemy"/>
<node CREATED="1648096769110" ID="ID_209384456" MODIFIED="1648536284083" TEXT="Aim at enemy"/>
<node CREATED="1648096777230" ID="ID_210484009" MODIFIED="1648536284083" TEXT="Move to ideal combat position"/>
</node>
<node CREATED="1648096789292" ID="ID_1029733810" MODIFIED="1648536284083" TEXT="Search for enemy">
<node CREATED="1648097770878" ID="ID_1526289067" MODIFIED="1648536284084" TEXT="Look around area with target&apos;s last known position"/>
<node CREATED="1648097784290" ID="ID_825161960" MODIFIED="1648536284084" TEXT="Move towards target&apos;s last known position"/>
</node>
</node>
<node CREATED="1648096711111" ID="ID_1257346392" MODIFIED="1648097365271" POSITION="right" TEXT="Investigate suspicious thing"/>
<node CREATED="1648096701952" ID="ID_1731258139" MODIFIED="1648536232555" POSITION="right">
<richcontent TYPE="NODE"><html>
  <head>
    
  </head>
  <body>
    <p>
      Patrol
    </p>
    <p>
      Prerequisites:
    </p>
    <p>
      Patrol route present<br />
    </p>
  </body>
</html>
</richcontent>
</node>
<node CREATED="1648096698393" ID="ID_207997366" MODIFIED="1648104258855" POSITION="right" TEXT="Idle/wander"/>
</node>
</map>
