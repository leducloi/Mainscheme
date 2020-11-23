/*
 * README - On creating maps
 * 
 * The map tiles that are automatically generated from the drawn tiles are critical to how the movement system works.
 * With this in mind, it is VERY important that the maps are made a certain way. The only requirement is that the maps
 * MUST start at 0,0 in the world and cannot go negative. This means that the maps must be rectangular in shape,
 * but if there needs to be "empty" tiles, we need to create a blank tile to ensure that the shape is rectangular and
 * starts from 0,0.
 */