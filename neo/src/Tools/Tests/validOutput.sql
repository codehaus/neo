
CREATE TABLE titles
(
	title_id CHAR (6) NOT NULL,
	title VARCHAR (80) NOT NULL,
	type VARCHAR (12) NOT NULL,
	pub_id CHAR (4) NULL,
	price DECIMAL  NULL,
	advance DECIMAL  NULL,
	royalty INTEGER  NULL,
	ytd_sales INTEGER  NULL,
	notes VARCHAR (200) NULL,
	pubdate DATETIME  NOT NULL,
	
		CONSTRAINT pk_titles PRIMARY KEY
		(
		title_id
		)
	
);
	
CREATE TABLE publishers
(
	pub_id CHAR (4) NOT NULL,
	pub_name VARCHAR (40) NULL,
	city VARCHAR (20) NULL,
	state CHAR (2) NULL,
	country VARCHAR (30) NULL,
	
		CONSTRAINT pk_publishers PRIMARY KEY
		(
		pub_id
		)
	
);
	
CREATE TABLE titleauthor
(
	title_id CHAR (6) NOT NULL,
	au_id CHAR (11) NOT NULL,
	
		CONSTRAINT pk_titleauthor PRIMARY KEY
		(
		title_id,au_id
		)
	
);
	
CREATE TABLE authors
(
	au_id CHAR (11) NOT NULL,
	au_lname VARCHAR (40) NOT NULL,
	au_fname VARCHAR (20) NOT NULL,
	phone CHAR (12) NOT NULL,
	contract BIT  NOT NULL,
	
		CONSTRAINT pk_authors PRIMARY KEY
		(
		au_id
		)
	
);
	
CREATE TABLE jobs
(
	job_id SMALLINT  NOT NULL,
	job_desc CHAR (50) NOT NULL,
	min_lvl TINYINT  NOT NULL,
	max_lvl TINYINT  NOT NULL
	
);
	
ALTER TABLE titles

		ADD CONSTRAINT fk_titles_Publisher FOREIGN KEY
		(
		pub_id
		)
		REFERENCES publishers
		(
		pub_id		
		)
;
		
ALTER TABLE titleauthor

		ADD CONSTRAINT fk_titleauthor_Titles FOREIGN KEY
		(
		title_id
		)
		REFERENCES titles
		(
		title_id		
		)
;
		
ALTER TABLE titleauthor

		ADD CONSTRAINT fk_titleauthor_Authors FOREIGN KEY
		(
		au_id
		)
		REFERENCES authors
		(
		au_id		
		)
;
		