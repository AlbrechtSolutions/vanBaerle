<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0"  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" >

  <xsl:output method="xml" omit-xml-declaration="yes" indent="yes"  encoding="utf-8" />
  <xsl:param name="html-content-type" />

  <xsl:template match="/NavigationTree">
     <xsl:if test="count(//Page) > 0">
      <li><a href="/">
          <xsl:value-of select="//LayoutNavigationSettings/@homepage" />
      </a>&#160;&#187;&#160;</li>
      <xsl:if test="count(//Page) > 0">
        <xsl:apply-templates select="//Page[@InPath='True']">
          <xsl:with-param name="depth" select="1"/>
        </xsl:apply-templates>
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <xsl:template match="//Page[@InPath='True']">
    <xsl:param name="depth"/>
      <li>
        <xsl:attribute name="data-href">
          <xsl:value-of select="@Href" disable-output-escaping="yes" />
        </xsl:attribute>
        <xsl:attribute name="class">
          <xsl:if test="@InPath = 'True'"> inpath</xsl:if>
          <xsl:if test="@Active = 'True'"> activeItem</xsl:if>
          <xsl:if test="@Allowclick='False'"> noClick</xsl:if>
        </xsl:attribute>
        <a href="javascript:void(0)">
          <xsl:if test="@Allowclick='True'">
            <xsl:attribute name="href"><xsl:value-of select="@Href" disable-output-escaping="yes" /></xsl:attribute>
            <xsl:if test="contains(@Href, 'http://')">
              <xsl:attribute name="target">_blank</xsl:attribute>
            </xsl:if>
          </xsl:if>
          <xsl:value-of select="@MenuText"/>
        </a>
        <xsl:if test="position() != last()">
          <xsl:text>&#160;&#187;&#160;</xsl:text>
        </xsl:if>
        <xsl:if test="@Active = 'False'">
        </xsl:if>
      </li>
  </xsl:template>
  
</xsl:stylesheet>