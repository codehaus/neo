<?xml version="1.0" encoding="UTF-8" ?>
<stylesheet version="1.0" xmlns="http://www.w3.org/1999/XSL/Transform">
<output method="text"/>
<strip-space elements="*"/>

<template match="/">
	<for-each select="//table">
CREATE TABLE <value-of select="./@name"/>
(
	<!--Build column definitions-->
	<apply-templates select="column" mode="TOP"/>
	<!--Build constraints-->
	<if test="count(column[@primaryKey = 'true']) > 0">
		CONSTRAINT pk_<value-of select="@name"/> PRIMARY KEY
		(
		<for-each select="column[@primaryKey = 'true']">
		<value-of select="@name"/>
		<if test="position() != last()">,</if>
		</for-each>
		)
	</if>
);
	</for-each>

	<for-each select="//table[count(foreign-key) > 0]">
		<for-each select="./foreign-key">
ALTER TABLE <value-of select="../@name"/>

		ADD CONSTRAINT fk_<value-of select="../@name"/>_<value-of select="@name"/> FOREIGN KEY
		(
		<for-each select="reference">
			<value-of select="@local"/>
			<if test="position() != last()">,</if>
		</for-each>
		)
		REFERENCES <value-of select="@foreignTable"/>
		(
		<for-each select="reference">
			<value-of select="@foreign"/>
			<if test="position() != last()">,</if>
		</for-each>		
		)
;
		</for-each>
	</for-each>
</template>

<template match="//column" mode="TOP">
	<value-of select="./@name"/><text> </text>
	<choose>
		<when test="@type='DATE'">
			<text>DATETIME</text>
		</when>
		<otherwise>
			<value-of select="./@type"/>
		</otherwise>
	</choose>
	<text> </text>
	<variable name="name" select="./@name"/>
	<if test="count(@size) = 1">(<value-of select="@size"/>)</if>
	<!--<apply-templates select="."/>-->
	<if test="@required = 'true'"> NOT</if> NULL<for-each select="../foreign-key">
		<!--
		<for-each select="reference">
			<if test="@local = $name">
		CONSTRAINT fk_<value-of select="$name"/><text> REFERENCES </text><value-of select="../@foreignTable"/> (<value-of select="@foreign"/>)</if>
		</for-each>
		-->
	</for-each>
	<if test="(count(../column[@primaryKey = 'true']) >= 1) or (position() != last())">,</if>
	<text>
	</text>
</template>

</stylesheet>