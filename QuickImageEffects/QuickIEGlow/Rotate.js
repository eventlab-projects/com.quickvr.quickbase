var speed = Vector3(10,15,19);

function Update () {
	transform.Rotate (speed * Time.deltaTime);
}