package com.hideakin.textsearch.index.controller;

import static org.mockito.Mockito.when;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

import org.hamcrest.Matchers;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.MockitoAnnotations;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.request.MockMvcRequestBuilders;
import org.springframework.test.web.servlet.setup.MockMvcBuilders;

import com.hideakin.textsearch.index.model.FileInfo;
import com.hideakin.textsearch.index.service.FileService;

@SpringBootTest
public class FileControllerTests {

	private MockMvc mockMvc;

	@Mock
	private FileService fileService;

	@InjectMocks
	private FileController fileController;

	@BeforeEach
	public void setUp() {
		MockitoAnnotations.initMocks(this);
		mockMvc = MockMvcBuilders.standaloneSetup(fileController).build();
	}

	@Test
	public void getFiles_successful() throws Exception {
		FileInfo[] fi = new FileInfo[] {
			new FileInfo(1001, "quux", 100, 10, "xyzzy"),
			new FileInfo(1002, "fred", 200, 10, "xyzzy"),
			new FileInfo(1003, "waldo", 300, 10, "xyzzy")
		};
		when(fileService.getFiles("xyzzy")).thenReturn(fi);
		mockMvc.perform(MockMvcRequestBuilders.get("/v1/files/xyzzy"))
        	.andExpect(status().isOk())
        	.andExpect(jsonPath("$[0].path").value("quux"))
        	.andExpect(jsonPath("$[1].path").value("fred"))
        	.andExpect(jsonPath("$[2].path").value("waldo"));
	}

	@Test
	public void getFiles_null() throws Exception {
		when(fileService.getFiles("xyzzy")).thenReturn(null);
		mockMvc.perform(MockMvcRequestBuilders.get("/v1/files/xyzzy"))
        	.andExpect(status().isNotFound());
	}

	@Test
	public void getFiles_empty() throws Exception {
		when(fileService.getFiles("xyzzy")).thenReturn(new FileInfo[0]);
		mockMvc.perform(MockMvcRequestBuilders.get("/v1/files/xyzzy"))
        	.andExpect(status().isOk())
        	.andExpect(jsonPath("$", Matchers.empty()));
	}

}
