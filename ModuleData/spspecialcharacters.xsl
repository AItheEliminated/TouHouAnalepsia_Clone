<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output omit-xml-declaration="yes" indent="yes"/>

	<xsl:template match="@* | node()">
		<xsl:copy>
			<xsl:apply-templates select="@* | node()"/>
		</xsl:copy>
	</xsl:template>
	<xsl:template match="@culture[parent::NPCCharacter and .='Culture.empire']">
		<xsl:attribute name="culture">
			<xsl:value-of select="'Culture.human'"/>
		</xsl:attribute>
	</xsl:template>
	<xsl:template match="@culture[parent::NPCCharacter and .='Culture.aserai']">
		<xsl:attribute name="culture">
			<xsl:value-of select="'Culture.fairy'"/>
		</xsl:attribute>
	</xsl:template>
	<xsl:template match="@culture[parent::NPCCharacter and .='Culture.sturgia']">
		<xsl:attribute name="culture">
			<xsl:value-of select="'Culture.human'"/>
		</xsl:attribute>
	</xsl:template>
	<xsl:template match="@culture[parent::NPCCharacter and .='Culture.vlandia']">
		<xsl:attribute name="culture">
			<xsl:value-of select="'Culture.human'"/>
		</xsl:attribute>
	</xsl:template>
	<xsl:template match="@culture[parent::NPCCharacter and .='Culture.battania']">
		<xsl:attribute name="culture">
			<xsl:value-of select="'Culture.youkai'"/>
		</xsl:attribute>
	</xsl:template>
	<xsl:template match="@culture[parent::NPCCharacter and .='Culture.khuzait']">
		<xsl:attribute name="culture">
			<xsl:value-of select="'Culture.youkai'"/>
		</xsl:attribute>
	</xsl:template>
	<xsl:template match="@culture[parent::NPCCharacter and .='Culture.nord']">
		<xsl:attribute name="culture">
			<xsl:value-of select="'Culture.youkai'"/>
		</xsl:attribute>
	</xsl:template>
	<xsl:template match="@culture[parent::NPCCharacter and .='Culture.vakken']">
		<xsl:attribute name="culture">
			<xsl:value-of select="'Culture.fairy'"/>
		</xsl:attribute>
	</xsl:template>
	<xsl:template match="@culture[parent::NPCCharacter and .='Culture.darshi']">
		<xsl:attribute name="culture">
			<xsl:value-of select="'Culture.fairy'"/>
		</xsl:attribute>
	</xsl:template>
	<xsl:template match="@culture[parent::NPCCharacter and .='Culture.looters']">
		<xsl:attribute name="culture">
			<xsl:value-of select="'Culture.fairy'"/>
		</xsl:attribute>
	</xsl:template>
	<xsl:template match="@culture[parent::NPCCharacter and .='Culture.sea_raiders']">
		<xsl:attribute name="culture">
			<xsl:value-of select="'Culture.youkai'"/>
		</xsl:attribute>
	</xsl:template>
	<xsl:template match="@culture[parent::NPCCharacter and .='Culture.mountain_bandits']">
		<xsl:attribute name="culture">
			<xsl:value-of select="'Culture.youkai'"/>
		</xsl:attribute>
	</xsl:template>
	<xsl:template match="@culture[parent::NPCCharacter and .='Culture.forrest_bandits']">
		<xsl:attribute name="culture">
			<xsl:value-of select="'Culture.youkai'"/>
		</xsl:attribute>
	</xsl:template>
	<xsl:template match="@culture[parent::NPCCharacter and .='Culture.desert_bandits']">
		<xsl:attribute name="culture">
			<xsl:value-of select="'Culture.youkai'"/>
		</xsl:attribute>
	</xsl:template>
	<xsl:template match="@culture[parent::NPCCharacter and .='Culture.steppe_bandits']">
		<xsl:attribute name="culture">
			<xsl:value-of select="'Culture.youkai'"/>
		</xsl:attribute>
	</xsl:template>
</xsl:stylesheet>