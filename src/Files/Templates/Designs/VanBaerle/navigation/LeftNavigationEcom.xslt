<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0"  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" >

	<xsl:output method="html" omit-xml-declaration="yes" indent="yes"  encoding="utf-8" />
	<xsl:param name="html-content-type" />
	<xsl:template match="/NavigationTree">

		<xsl:if test="count(//Page[@InPath='True']/Page) > 0">
			<ul>
				<xsl:if test="Settings/LayoutNavigationAttributes/@class!=''">
					<xsl:attribute name="class">
						<xsl:value-of select="Settings/LayoutNavigationAttributes/@class" disable-output-escaping="yes"/>
                        <xsl:if test="count(//Page[@InPath='True']/Page/Page[@Type!='product']) = -1"> l2 active</xsl:if>
					</xsl:attribute>
				</xsl:if>
				<xsl:if test="Settings/LayoutNavigationAttributes/@id!=''">
					<xsl:attribute name="id">
						<xsl:value-of select="Settings/LayoutNavigationAttributes/@id" disable-output-escaping="yes"/>
					</xsl:attribute>
				</xsl:if>
				
				<xsl:apply-templates select="Page[@InPath='True']/Page">
					<xsl:with-param name="depth" select="3"/>
                    <xsl:with-param name="nodes" select="count(Page[@InPath='True']/Page)"/>
				</xsl:apply-templates>
			</ul>
		</xsl:if>

	</xsl:template>

	<xsl:template match="//Page">
		<xsl:param name="depth"/>
      	<xsl:param name="nodes"/>
		<li>
			<xsl:attribute name="class">
				<xsl:if test="count(Page) and @InPath='True' and /NavigationTree/Settings/LayoutNavigationSettings/@endlevel > @RelativeLevel">list-open-active </xsl:if>
				<xsl:if test="@Active='True'">list-active </xsl:if>
                <xsl:choose>
                  <xsl:when test="count(Page [@Type!='product'])">
                    <xsl:value-of select="concat('l',@AbsoluteLevel)"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="concat('l',@AbsoluteLevel+1)"/>
                  </xsl:otherwise>
              </xsl:choose>
                <xsl:if test="$nodes='1'"> active</xsl:if>
			</xsl:attribute>
			<a>
				<xsl:attribute name="href">
					<xsl:value-of select="@FriendlyHref" disable-output-escaping="yes"/>
				</xsl:attribute>  

                <xsl:if test="contains(@FriendlyHref, 'http')">
                  <xsl:attribute name="target">_blank</xsl:attribute>
                </xsl:if>
				<xsl:value-of select="@MenuText" disable-output-escaping="yes"/>
				<span></span>
			</a>

			
			
			<!-- <xsl:if test="count(Page) and @InPath='True' and /NavigationTree/Settings/LayoutNavigationSettings/@endlevel > @RelativeLevel"> -->
			<xsl:if test="count(Page [@Type!='product'])">
				<ul>
					<xsl:attribute name="class">
						<xsl:if test="count(Page[@Type='group']) and @InPath='True' and /NavigationTree/Settings/LayoutNavigationSettings/@endlevel > @RelativeLevel">selected </xsl:if>
                         <xsl:value-of select="concat('M',@AbsoluteLevel)"/>
					</xsl:attribute>
                  <xsl:attribute name="style">
                    <xsl:if test="$nodes='1'"><xsl:value-of select="concat('display',':','block')" disable-output-escaping="yes"/></xsl:if>
                  </xsl:attribute>
					<xsl:apply-templates select="Page[@Type='group']">
						<xsl:with-param name="depth" select="$depth+1"/>
                      <xsl:with-param name="nodes" select="$nodes"/>
					</xsl:apply-templates>
				</ul>
			</xsl:if>
		</li>
	</xsl:template>

</xsl:stylesheet>