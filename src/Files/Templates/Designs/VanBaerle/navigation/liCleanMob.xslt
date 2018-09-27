<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:output method="html" omit-xml-declaration="yes" indent="yes" encoding="utf-8" />
  <xsl:param name="html-content-type" />
  <xsl:template match="/NavigationTree">

    <xsl:if test="count(//Page[@ItemType!='Tab' or string-length(@ItemType)=0]) > 0">
      <ul class="nav navbar-nav main-nav__nav pageLevel{//Page[position() = 1]/@AbsoluteLevel}">
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
      <xsl:if test="position() = last()">
        <xsl:attribute name="data-position">lastItem</xsl:attribute>
      </xsl:if>
      <xsl:attribute name="data-dropdown">
        <xsl:value-of select="@MegaMenu" disable-output-escaping="yes"/>
      </xsl:attribute>
      <xsl:attribute name="data-submenulinks">
        <xsl:value-of select="@Submenu_Links" disable-output-escaping="yes"/>
      </xsl:attribute>
      <xsl:if test="position() = 1 or @Class!='' or position() = last() or @InPath = 'True' or @Active = 'True' or count(Page[@ItemType!='Tab' or string-length(@ItemType)=0]) > 0 or @Allowclick = 'False'">
        <xsl:attribute name="class">
          <xsl:if test="@Class != ''">
            <xsl:value-of select="@Class" disable-output-escaping="yes" />
          </xsl:if>
          <xsl:if test="position() = 1"> firstItem</xsl:if>
          <xsl:if test="position() = last() and position() != 1"> lastItem</xsl:if>
          <!-- <xsl:if test="@InPath = 'True'"> inpath</xsl:if> -->
          <xsl:if test="@Active = 'True'"> active</xsl:if>
          <xsl:if test="count(Page[@ItemType!='Tab' or string-length(@ItemType)=0]) > 0"> hasChildren dropdown</xsl:if>
          <xsl:if test="@Allowclick = 'False'"> noClick</xsl:if>
        </xsl:attribute>
      </xsl:if>
      <xsl:choose>
        <xsl:when test="count(Page[@ItemType!='Tab' or string-length(@ItemType)=0]) > 0">
          <a href="#" class="dropdown-toggle hidden-lg hidden-md {concat('dropdown-toggle-',@AbsoluteLevel)}" role="button" aria-expanded="false"><span class="caret"></span></a>
          <a href="javascript:void(0)">
            <xsl:if test="@Allowclick = 'True'">
              <xsl:attribute name="href">
                <!--<xsl:value-of select="@Href" disable-output-escaping="yes" /><xsl:if test="@Category != ''">?GroupID=<xsl:value-of select="@Category" disable-output-escaping="yes" /></xsl:if><xsl:if test="@SmartSearch != ''"><xsl:if test="@SmartSearch != 'False'">&amp;NewRelease=<xsl:value-of select="@SmartSearch" disable-output-escaping="yes" /></xsl:if></xsl:if><xsl:if test="@Category != ''">&amp;Category=<xsl:value-of select="@Category" disable-output-escaping="yes" />&amp;sortby=ProductCategory_LorenzAx_ReleaseDate&amp;SortOrder=DESC</xsl:if><xsl:if test="@SubCategory != ''">&amp;Subcategory=<xsl:value-of select="@SubCategory" disable-output-escaping="yes" /></xsl:if>-->
                <xsl:value-of select="@Href" disable-output-escaping="yes" /><xsl:if test="@AnchorLink != ''">#<xsl:value-of select="@AnchorLink" disable-output-escaping="yes" /></xsl:if>
              </xsl:attribute>
              <xsl:if test="contains(@Href, 'http://')">
                <xsl:attribute name="target">_blank</xsl:attribute>
              </xsl:if>
              </xsl:if>
              <xsl:if test="@AbsoluteLevel = 2">
            </xsl:if>
            <xsl:if test="@MenuIcon!=''">
              <span>
                <xsl:attribute name="class">
                  <xsl:value-of select="@MenuIcon" disable-output-escaping="yes" />
                </xsl:attribute>
                 &#160;
              </span>
            </xsl:if>
            <span class="menuText textActions">
              <xsl:value-of select="@MenuText" disable-output-escaping="yes" />

              <xsl:if test="count(Page[@ItemType!='Tab' or string-length(@ItemType)=0]) > 0 and @AbsoluteLevel &gt; 1">
                <i class="icon vb-triangle-right"></i>
              </xsl:if>
            </span>
          </a>
        </xsl:when>
        <xsl:otherwise>
          <a href="javascript:void(0)">
            <xsl:if test="@Allowclick = 'True'">
              <xsl:attribute name="href">
                <!--<xsl:value-of select="@Href" disable-output-escaping="yes" /><xsl:if test="@Category != ''">?GroupID=<xsl:value-of select="@Category" disable-output-escaping="yes" /></xsl:if><xsl:if test="@SmartSearch != ''"><xsl:if test="@SmartSearch != 'False'">&amp;NewRelease=<xsl:value-of select="@SmartSearch" disable-output-escaping="yes" /></xsl:if></xsl:if><xsl:if test="@Category != ''">&amp;Category=<xsl:value-of select="@Category" disable-output-escaping="yes" />&amp;sortby=ProductCategory_LorenzAx_ReleaseDate&amp;SortOrder=DESC</xsl:if><xsl:if test="@SubCategory != ''">&amp;Subcategory=<xsl:value-of select="@SubCategory" disable-output-escaping="yes" /></xsl:if>-->
                <xsl:value-of select="@Href" disable-output-escaping="yes" /><xsl:if test="@AnchorLink != ''">#<xsl:value-of select="@AnchorLink" disable-output-escaping="yes" /></xsl:if>
              </xsl:attribute>
              <xsl:if test="contains(@Href, 'http://')">
                <xsl:attribute name="target">_blank</xsl:attribute>
              </xsl:if>
              </xsl:if>
              <xsl:if test="@AbsoluteLevel = 2">
            </xsl:if>
            <xsl:if test="@MenuIcon!=''">
              <span>
                <xsl:attribute name="class">
                  <xsl:value-of select="@MenuIcon" disable-output-escaping="yes" />
                </xsl:attribute>
                &#160;
              </span>
            </xsl:if>
            <span class="subMenuText textActions">
              <xsl:value-of select="@MenuText" disable-output-escaping="yes" />
            </span>
          </a>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:if test="count(Page[@ItemType!='Tab' or string-length(@ItemType)=0]) > 0">
          <ul class="dropdown-menu {concat('dropdown-menu-',@AbsoluteLevel)}" role="menu">

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