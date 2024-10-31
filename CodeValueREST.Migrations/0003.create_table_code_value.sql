create table code_value (
    id    serial not null,
    code  integer PRIMARY KEY,
    value varchar not null
);

comment on table code_value is 'Код Значение';
comment on column code_value.id is 'Порядковый номер';
comment on column code_value.code is 'Код';
comment on column code_value.value is 'Значение';
