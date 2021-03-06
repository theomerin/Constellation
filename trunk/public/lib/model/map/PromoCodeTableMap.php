<?php


/**
 * This class defines the structure of the 'promo_code' table.
 *
 *
 * This class was autogenerated by Propel 1.4.2 on:
 *
 * Thu 26 Apr 2012 01:25:35 PM EDT
 *
 *
 * This map class is used by Propel to do runtime db structure discovery.
 * For example, the createSelectSql() method checks the type of a given column used in an
 * ORDER BY clause to know whether it needs to apply SQL to make the ORDER BY case-insensitive
 * (i.e. if it's a text column type).
 *
 * @package    lib.model.map
 */
class PromoCodeTableMap extends TableMap {

	/**
	 * The (dot-path) name of this class
	 */
	const CLASS_NAME = 'lib.model.map.PromoCodeTableMap';

	/**
	 * Initialize the table attributes, columns and validators
	 * Relations are not initialized by this method since they are lazy loaded
	 *
	 * @return     void
	 * @throws     PropelException
	 */
	public function initialize()
	{
	  // attributes
		$this->setName('promo_code');
		$this->setPhpName('PromoCode');
		$this->setClassname('PromoCode');
		$this->setPackage('lib.model');
		$this->setUseIdGenerator(true);
		// columns
		$this->addPrimaryKey('PROMO_CODE_ID', 'PromoCodeId', 'INTEGER', true, 11, null);
		$this->addColumn('PROMO_CODE_TYPE', 'PromoCodeType', 'TINYINT', false, 4, null);
		$this->addColumn('PROMO_CODE_VALUE', 'PromoCodeValue', 'FLOAT', false, null, null);
		$this->addColumn('PROMO_CODE_CODE', 'PromoCodeCode', 'VARCHAR', false, 255, null);
		$this->addColumn('FK_FILM_ID', 'FkFilmId', 'INTEGER', false, 11, null);
		$this->addColumn('PROMO_CODE_USES', 'PromoCodeUses', 'INTEGER', false, 11, null);
		$this->addColumn('PROMO_CODE_TOTAL_USAGE', 'PromoCodeTotalUsage', 'INTEGER', false, 11, null);
		// validators
	} // initialize()

	/**
	 * Build the RelationMap objects for this table relationships
	 */
	public function buildRelations()
	{
	} // buildRelations()

	/**
	 * 
	 * Gets the list of behaviors registered for this table
	 * 
	 * @return array Associative array (name => parameters) of behaviors
	 */
	public function getBehaviors()
	{
		return array(
			'symfony' => array('form' => 'true', 'filter' => 'true', ),
			'symfony_behaviors' => array(),
		);
	} // getBehaviors()

} // PromoCodeTableMap
