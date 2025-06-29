program example;

var
  x: integer;
  y: real;

function sum(a: integer; b: integer): real;
begin
  sum := a + b;
end;

begin
  x := 10;
  y := sum(x, 20);
end.
