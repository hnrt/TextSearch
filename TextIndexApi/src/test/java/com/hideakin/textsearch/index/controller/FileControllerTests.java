package com.hideakin.textsearch.index.controller;

import static org.mockito.Mockito.when;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.MockitoAnnotations;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.request.MockMvcRequestBuilders;
import org.springframework.test.web.servlet.setup.MockMvcBuilders;

import com.hideakin.textsearch.index.model.ValuesResponse;
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
		ValuesResponse rsp = new ValuesResponse();
		rsp.setValues(new String[] { "quux", "fred", "waldo" });
		when(fileService.getFiles("default")).thenReturn(rsp);
		mockMvc.perform(MockMvcRequestBuilders.get("/files"))
        	.andExpect(status().isOk())
        	.andExpect(jsonPath("$.values[0]").value(rsp.getValues()[0]))
        	.andExpect(jsonPath("$.values[1]").value(rsp.getValues()[1]))
        	.andExpect(jsonPath("$.values[2]").value(rsp.getValues()[2]));
	}

	@Test
	public void getFilesByGroup_successful() throws Exception {
		ValuesResponse rsp = new ValuesResponse();
		rsp.setValues(new String[] { "corge", "grault", "garply" });
		when(fileService.getFiles("xyzzy")).thenReturn(rsp);
		mockMvc.perform(MockMvcRequestBuilders.get("/files/xyzzy"))
        	.andExpect(status().isOk())
        	.andExpect(jsonPath("$.values[0]").value(rsp.getValues()[0]))
        	.andExpect(jsonPath("$.values[1]").value(rsp.getValues()[1]))
        	.andExpect(jsonPath("$.values[2]").value(rsp.getValues()[2]));
	}

}
