# Height Map Generator DLA Based 
This an attempt to create a Height Map generator using DLA, at this moment is written in c#.
Given size of the image and number of points it outputs a png with a corresponding heightmap in greyscale. 

## Presentation
Hello, this is my first serious repository, so it will probably full of problems.
I'm not an expert programmer and i lack quite some knowledge in efficient mathematical implementation, but i'm aware of the incredible amount of garbage that you will see in my code.

If anyone is (somehow) interested in trying to optimize or better this code i will be more than happy (and suprised).
Below i will leave my inspirations and help.

## Considerations
- At this moment is written in *c#* and i know is not very efficient.
- A lot of redundancy is present in the code.
- **No user input is implemented, so you must change the values inside ```Program.cs``` and then recompile it to make changes to the output**.
- They may exist some unhandled exceptions.
- It's single threaded (at the moment).
- There is only 1 peak of the mountain and it's at the center of the matrix.
- **ITS COMPLEXITY RAISES EXTREMELY STEEPLY (PROBABLY SOMETHING LIKE O(n^10)) BECAUSE OF HOW SHITTLY I IMPLEMENTED IT**

# CREDITS
I want to thank specially [Josh](https://www.youtube.com/@JoshsHandle), for his inspiration and guidelines provided in his [video](https://www.youtube.com/watch?v=gsJHzBTPG0Y&list=PLZLW2nBDX-Xs3NphGwbMZlPltF7GIXCRX&index=38); if you read this, i'm sorry, this is a terrible version of your idea.

I want to say that i used also the help of AI and i don't want to hide it, particularly ChatGPT and TabNine.

Also i want to mention [@BrutPitt](https://github.com/BrutPitt) for his [implementation](https://github.com/BrutPitt/DLAf-optimized) of DLA, which helped me understand it a little more.

Thanks to all the user online and the internet.
