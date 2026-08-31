BEGIN TRANSACTION;

SELECT * FROM range_visit_counts WHERE range_name = 'Fake Range';
SELECT * FROM firearm_round_counts WHERE firearm_name = 'Test Gun';
SELECT * FROM fi_doc_firearm WHERE json_extract(data, '$.name') = 'Test Gun';

ROLLBACK;