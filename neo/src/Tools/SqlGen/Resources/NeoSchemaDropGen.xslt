<?xml version="1.0" encoding="UTF-8" ?>
<stylesheet version="1.0" xmlns="http://www.w3.org/1999/XSL/Transform">
<output method="text"/>
<strip-space elements="*"/>

<template match="/">
	<for-each select="//table[count(foreign-key) > 0]">
		<for-each select="./foreign-key">
ALTER TABLE <value-of select="../@name"/>

		DROP CONSTRAINT fk_<value-of select="../@name"/>_<value-of select="@name"/> 
;
		</for-each>
	</for-each>
	<for-each select="//table">
DROP TABLE <value-of select="./@name"/>
	</for-each>
</template>

</stylesheet>
