<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:output method="xml" omit-xml-declaration="yes" indent="yes" encoding="utf-8" />
  <xsl:param name="html-content-type" />
  <xsl:template match="/NavigationTree">

    <xsl:if test="count(//Page[@ItemType!='Tab' or string-length(@ItemType)=0]) > 0">
      <ul class="pageLevel{//Page[position() = 1]/@AbsoluteLevel}">
        <xsl:if test="string-length(//Pageview/@NavigationName) > 0">
          <xsl:attribute name="id">
            <xsl:value-of select="//Pageview/@NavigationName" />
          </xsl:attribute>
        </xsl:if>
        <xsl:apply-templates select="Page">
          <xsl:with-param name="depth" select="1" />
        </xsl:apply-templates>
      </ul>
    </xsl:if>

  </xsl:template>

  <xsl:template match="Page">
    <xsl:param name="depth" />

    <xsl:variable name="imageActive">
      <xsl:call-template name="filePathCorrection">
        <xsl:with-param name="dbPath">
          <xsl:value-of select="@ImageActive" /></xsl:with-param>
      </xsl:call-template>
    </xsl:variable>

    <li>
      <xsl:if test="position() = 1 or position() = last() or @InPath = 'True' or @Active = 'True' or count(Page[@ItemType!='Tab' or string-length(@ItemType)=0]) > 0 or @Allowclick = 'False'">
        <xsl:attribute name="class">
          <xsl:if test="@Class != ''">
            <xsl:value-of select="@Class" disable-output-escaping="yes" />
          </xsl:if>
          <xsl:if test="position() = 1"> firstItem</xsl:if>
          <xsl:if test="position() = last() and position() != 1"> lastItem</xsl:if>
          <xsl:if test="@InPath = 'True'"> inpath</xsl:if>
          <xsl:if test="@Active = 'True'"> activeItem</xsl:if>
          <xsl:if test="count(Page[@ItemType!='Tab' or string-length(@ItemType)=0]) > 0"> hasChildren</xsl:if>
          <xsl:if test="@Allowclick = 'False'"> noClick</xsl:if>
        </xsl:attribute>
      </xsl:if>
      <a href="javascript:void(0)">
        <xsl:if test="@Allowclick = 'True'">
          <xsl:attribute name="href">
            <xsl:value-of select="@Href" disable-output-escaping="yes" />
          </xsl:attribute>
          <xsl:if test="contains(@Href, 'http://')">
            <xsl:attribute name="target">_blank</xsl:attribute>
          </xsl:if>
        </xsl:if>
        <xsl:if test="@AbsoluteLevel = 2">
        </xsl:if>
        <xsl:if test="@Icon!=''">
              <span>
                <xsl:attribute name="class">
                  <xsl:value-of select="@Icon" disable-output-escaping="yes" />
                </xsl:attribute>
                 &#160;
              </span>
            </xsl:if>
        <xsl:value-of select="@MenuText" disable-output-escaping="yes" />
      </a>
      <xsl:if test="count(Page[@ItemType!='Tab' or string-length(@ItemType)=0]) > 0">
          <ul class="pageLevel{@AbsoluteLevel+1}">
            <xsl:if test="@ImageActive != ''">
              <xsl:attribute name="data-image">
                <xsl:value-of select="$imageActive" disable-output-escaping="no" />
              </xsl:attribute>
              <xsl:attribute name="class">image</xsl:attribute>
            </xsl:if>
            <xsl:apply-templates select="Page">
              <xsl:with-param name="depth" select="$depth+1" />
            </xsl:apply-templates>
          </ul>
      </xsl:if>
    </li>
  </xsl:template>

  <xsl:template name="filePathCorrection">
    <xsl:param name="dbPath" />

    <xsl:choose>
      <xsl:when test="contains($dbPath,'/Navigation/..')">
        <xsl:text>/Files</xsl:text><xsl:value-of select="substring-after($dbPath,'/Navigation/..')" />
      </xsl:when>
      <xsl:otherwise><xsl:value-of select="$dbPath" /></xsl:otherwise>
    </xsl:choose>
  </xsl:template>

</xsl:stylesheet>