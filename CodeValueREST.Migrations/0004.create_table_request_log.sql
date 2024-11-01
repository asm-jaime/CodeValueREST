create table request_log (
    id             uuid         primary key,
    request_time   timestamp,
    response_time  timestamp,
    request_url    varchar(255) not null,
    response_code  integer      not null,
    request_size   integer      not null,
    response_size  integer      not null
);

comment on table request_log is 'Лог запросов';
comment on column request_log.id is 'Идентификатор';
comment on column request_log.request_time is 'Время получения запроса';
comment on column request_log.response_time is 'Время ответа на запрос';
comment on column request_log.request_url is 'URL входящего запроса';
comment on column request_log.response_code is 'Код ответа';
comment on column request_log.request_size is 'Размер запроса в байтах';
comment on column request_log.response_size is 'Размер ответа в байтах';
